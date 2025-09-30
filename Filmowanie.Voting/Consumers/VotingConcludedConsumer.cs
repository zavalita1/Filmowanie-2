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
public sealed class VotingConcludedConsumer : IConsumer<VotingConcludedEvent>, IConsumer<Fault<VotingConcludedEvent>>
{
    private readonly ILogger<VotingConcludedConsumer> logger;
    private readonly IVotingResultsCommandRepository votingResultsCommandRepository;
    private readonly IRepositoryInUserlessContextProvider repositoriesProvider;
    private readonly IDateTimeProvider dateTimeProvider;
    private readonly IPickUserToNominateContextRetriever pickUserToNominateContextRetriever;
    private readonly IVotingResultsRetriever votingResultsRetriever;
    private readonly INominationsRetriever nominationsRetriever;
    private readonly IUsersQueryRepository usersQueryRepository;
    private readonly IBus bus;
    private const int DecidersTimeWindow = 10;

    public VotingConcludedConsumer(ILogger<VotingConcludedConsumer> logger, IVotingResultsCommandRepository votingResultsCommandRepository, IDateTimeProvider dateTimeProvider, IRepositoryInUserlessContextProvider votingResultsRepositoryProvider, IPickUserToNominateContextRetriever pickUserToNominateContextRetriever, IVotingResultsRetriever votingResultsRetriever, INominationsRetriever nominationsRetriever, IUsersQueryRepository usersQueryRepository, IBus bus)
    {
        this.logger = logger;
        this.votingResultsCommandRepository = votingResultsCommandRepository;
        this.dateTimeProvider = dateTimeProvider;
        this.repositoriesProvider = votingResultsRepositoryProvider;
        this.pickUserToNominateContextRetriever = pickUserToNominateContextRetriever;
        this.votingResultsRetriever = votingResultsRetriever;
        this.nominationsRetriever = nominationsRetriever;
        this.usersQueryRepository = usersQueryRepository;
        this.bus = bus;
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
            var currentVotingSessionId = context.Message.VotingSessionId;

            var currentVotingResults = GetCurrentVotingResults(message, lastVotingResult);

            var moviesAdded = GetMoviesAddedDuringSession(message);
            var assignNominationsUserContexts = this.pickUserToNominateContextRetriever.GetPickUserToNominateContexts(lastFewVotingResults, moviesAdded, message);
            var nominations = this.nominationsRetriever.GetNominations(assignNominationsUserContexts, message, currentVotingResults);
            var enrichedWinnerEntity = await EnrichWinnerEntityAsync(message.Tenant, currentVotingResults.Winner, context.CancellationToken);

            var updateResult = await this.votingResultsCommandRepository.UpdateAsync(currentVotingSessionId, currentVotingResults.Movies, nominations, now, moviesAdded, enrichedWinnerEntity, currentVotingResults.MoviesGoingByeBye, context.CancellationToken);

            if (updateResult.Error.HasValue)
                await PublishErrorAsync(context, error: updateResult.Error);

            this.logger.LogInformation($"Consumed {nameof(VotingConcludedEvent)} event.");
        }
        catch (Exception ex)
        {
            await PublishErrorAsync(context, ex);
        }
    }

    private VotingResults GetCurrentVotingResults(VotingConcludedEvent message, IReadOnlyVotingResult? lastVotingResult)
    {
        var votingResults = this.votingResultsRetriever.GetVotingResults(message.MoviesWithVotes, lastVotingResult);

        if (message.ExtraVotingMoviesWithVotes != null)
        {
            var extraVotingResults = this.votingResultsRetriever.GetVotingResults(message.ExtraVotingMoviesWithVotes, lastVotingResult);
            var winner = votingResults.Movies.Single(x => x.Movie.id == extraVotingResults.Winner.id);
            votingResults = votingResults with { Movies = [winner, .. votingResults.Movies.Except([winner])], Winner = extraVotingResults.Winner };
        }

        return votingResults;
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

    private async Task<EmbeddedMovieWithNominationContext> EnrichWinnerEntityAsync(TenantId tenant, IReadOnlyEmbeddedMovie winner, CancellationToken cancelToken)
    {
        var movieId = new MovieId(winner.id);
        var repo = this.repositoriesProvider.GetMovieDomainRepository(tenant);
        var nominatedEvents = await repo.GetMovieNominatedEventsAsync(movieId, cancelToken);
        var mostRecentNominatedAgainEvent = nominatedEvents.MaxBy(x => x.Created);
        var nominatingUser = await this.usersQueryRepository.GetUserAsync(x => x.id == mostRecentNominatedAgainEvent!.UserId, cancelToken);
        return new EmbeddedMovieWithNominationContext(winner) { NominatedBy = new EmbeddedUser(nominatingUser!) };
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