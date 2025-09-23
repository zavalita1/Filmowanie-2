using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Consumers;

// TODO UTs
public sealed class ResultsConfirmedConsumer : IConsumer<VotingConcludedEvent>, IConsumer<Fault<VotingConcludedEvent>>
{
    private readonly ILogger<ResultsConfirmedConsumer> logger;
    private readonly IVotingResultsCommandRepository votingResultsCommandRepository;
    private readonly IRepositoryInUserlessContextProvider repositoriesProvider;
    private readonly IDateTimeProvider dateTimeProvider;
    private readonly IPickUserToNominateContextRetriever pickUserToNominateContextRetriever;
    private readonly IVotingResultsRetriever votingResultsRetriever;
    private readonly INominationsRetriever nominationsRetriever;
    private readonly IUsersQueryRepository usersQueryRepository;
    private const int DecidersTimeWindow = 10;

    public ResultsConfirmedConsumer(ILogger<ResultsConfirmedConsumer> logger, IVotingResultsCommandRepository votingResultsCommandRepository, IDateTimeProvider dateTimeProvider, IRepositoryInUserlessContextProvider votingResultsRepositoryProvider, IPickUserToNominateContextRetriever pickUserToNominateContextRetriever, IVotingResultsRetriever votingResultsRetriever, INominationsRetriever nominationsRetriever, IUsersQueryRepository usersQueryRepository)
    {
        this.logger = logger;
        this.votingResultsCommandRepository = votingResultsCommandRepository;
        this.dateTimeProvider = dateTimeProvider;
        this.repositoriesProvider = votingResultsRepositoryProvider;
        this.pickUserToNominateContextRetriever = pickUserToNominateContextRetriever;
        this.votingResultsRetriever = votingResultsRetriever;
        this.nominationsRetriever = nominationsRetriever;
        this.usersQueryRepository = usersQueryRepository;
    }

    public Task Consume(ConsumeContext<Fault<VotingConcludedEvent>> context)
    {
        var message = context.Message.Exceptions.Select(x => x.Message).JoinStrings();
        var callStacks = context.Message.Exceptions.Select(x => x.StackTrace).JoinStrings(";;;;;;;;;;;;");
        return context.Publish(new ErrorEvent(context.Message.Message.VotingSessionId, message, callStacks));
    }

    public async Task Consume(ConsumeContext<VotingConcludedEvent> context)
    {
        this.logger.LogInformation($"Consuming {nameof(VotingConcludedEvent)}...");

        try
        {
            var now = dateTimeProvider.Now;
            var message = context.Message;

            var repo = this.repositoriesProvider.GetVotingResultsRepository(message.Tenant);
            var lastFewVotingResults = (await repo.GetLastNVotingResultsAsync(DecidersTimeWindow, context.CancellationToken)).RequireResult().ToArray();
            var lastVotingResult = lastFewVotingResults.FirstOrDefault(); // null only if it's initial voting
            var votingResults = this.votingResultsRetriever.GetVotingResults(message.MoviesWithVotes, lastVotingResult);

            var moviesAdded = GetMoviesAddedDuringSession(message);

            var assignNominationsUserContexts = this.pickUserToNominateContextRetriever.GetPickUserToNominateContexts(lastFewVotingResults, moviesAdded, message);
            var nominations = this.nominationsRetriever.GetNominations(assignNominationsUserContexts, message, votingResults);

            var currentVotingSessionId = context.Message.VotingSessionId;

            var enrichedWinnerEntity = await EnrichWinnerEntityAsync(message.Tenant, votingResults, context.CancellationToken);
            var updateResult = await this.votingResultsCommandRepository.UpdateAsync(currentVotingSessionId, votingResults.Movies, nominations, now, moviesAdded, enrichedWinnerEntity, votingResults.MoviesGoingByeBye, context.CancellationToken);

            if (updateResult.Error.HasValue)
                await PublishErrorAsync(context, error: updateResult.Error);

            this.logger.LogInformation($"Consumed {nameof(VotingConcludedEvent)} event.");
        }
        catch (Exception ex)
        {
            await PublishErrorAsync(context, ex);
        }
    }

    private Task PublishErrorAsync(ConsumeContext<VotingConcludedEvent> context, Exception? ex = null, Error<VoidResult>? error = null)
    {
        var msg = "Error occurred during concluding the voting..." + error;

        if (ex == null)
            this.logger.LogError(msg);
        else
            this.logger.LogError(ex, msg);

        var errorEvent = new ErrorEvent(context.Message.VotingSessionId, msg, ex?.StackTrace ?? "Unknown");
        return context.Publish(errorEvent, context.CancellationToken);
    }

    private async Task<EmbeddedMovieWithNominationContext> EnrichWinnerEntityAsync(TenantId tenant, VotingResults votingResults, CancellationToken cancelToken)
    {
        var movieId = new MovieId(votingResults.Winner.id);
        var repo = this.repositoriesProvider.GetMovieDomainRepository(tenant);
        var nominatedEvents = await repo.GetMovieNominatedEventsAsync(movieId, cancelToken);
        var mostRecentNominatedAgainEvent = nominatedEvents.MaxBy(x => x.Created);
        var nominatingUser = await this.usersQueryRepository.GetUserAsync(x => x.id == mostRecentNominatedAgainEvent!.UserId, cancelToken);
        return new EmbeddedMovieWithNominationContext(votingResults.Winner) { NominatedBy = new EmbeddedUser(nominatingUser!) };
    }

    private static IReadOnlyEmbeddedMovieWithNominationContext[] GetMoviesAddedDuringSession(VotingConcludedEvent message)
    {
        return message.NominationsData.Join(message.MoviesWithVotes, x => x.MovieId, x => x.Movie.id, IReadOnlyEmbeddedMovieWithNominationContext (x, y) => new ReadOnlyEmbeddedMovieWithNominationContext
        (
            y.Movie,
            new EmbeddedUser { id = x!.User!.Id!, Name = x!.User!.DisplayName!, TenantId = message.Tenant.Id },
            x.Concluded!.Value,
            message.VotingStarted
        )).ToArray();
    }

    private readonly record struct ReadOnlyEmbeddedMovieWithNominationContext(IReadOnlyEmbeddedMovie Movie, IReadOnlyEmbeddedUser NominatedBy, DateTime NominationConcluded, DateTime NominationStarted)
        : IReadOnlyEmbeddedMovieWithNominationContext;
}