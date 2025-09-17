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
    private readonly ILogger<VotingConcludedConsumer> _logger;
    private readonly IVotingResultsCommandRepository _votingResultsCommandRepository;
    private readonly IRepositoryInUserlessContextProvider _repositoriesProvider;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPickUserToNominateContextRetriever _pickUserToNominateContextRetriever;
    private readonly IVotingResultsRetriever _votingResultsRetriever;
    private readonly INominationsRetriever _nominationsRetriever;
    private readonly IUsersQueryRepository _usersQueryRepository;
    private const int DecidersTimeWindow = 10;

    public VotingConcludedConsumer(ILogger<VotingConcludedConsumer> logger, IVotingResultsCommandRepository votingResultsCommandRepository, IDateTimeProvider dateTimeProvider, IRepositoryInUserlessContextProvider votingResultsRepositoryProvider, IPickUserToNominateContextRetriever pickUserToNominateContextRetriever, IVotingResultsRetriever votingResultsRetriever, INominationsRetriever nominationsRetriever, IMovieDomainRepository movieDomainRepository, IUsersQueryRepository usersQueryRepository)
    {
        _logger = logger;
        _votingResultsCommandRepository = votingResultsCommandRepository;
        _dateTimeProvider = dateTimeProvider;
        _repositoriesProvider = votingResultsRepositoryProvider;
        _pickUserToNominateContextRetriever = pickUserToNominateContextRetriever;
        _votingResultsRetriever = votingResultsRetriever;
        _nominationsRetriever = nominationsRetriever;
        _usersQueryRepository = usersQueryRepository;
    }

    public Task Consume(ConsumeContext<Fault<VotingConcludedEvent>> context)
    {
        var message = string.Join(",", context.Message.Exceptions.Select(x => x.Message));
        var callStacks = string.Join(";;;;;;;;;;;;", context.Message.Exceptions.Select(x => x.StackTrace));
        return context.Publish(new ErrorEvent(context.Message.Message.VotingSessionId, message, callStacks));
    }

    public async Task Consume(ConsumeContext<VotingConcludedEvent> context)
    {
        _logger.LogInformation($"Consuming {nameof(VotingConcludedEvent)}...");

        try
        {
            var now = _dateTimeProvider.Now;
            var message = context.Message;

            var repo = _repositoriesProvider.GetVotingResultsRepository(message.Tenant);
            var lastFewVotingResults = (await repo.GetLastNVotingResultsAsync(DecidersTimeWindow, context.CancellationToken)).RequireResult().ToArray();
            var lastVotingResult = lastFewVotingResults.FirstOrDefault(); // null only if it's initial voting
            var votingResults = _votingResultsRetriever.GetVotingResults(message.MoviesWithVotes, lastVotingResult);

            var moviesAdded = GetMoviesAddedDuringSession(message);

            var assignNominationsUserContexts = _pickUserToNominateContextRetriever.GetPickUserToNominateContexts(lastFewVotingResults, moviesAdded, message);
            var nominations = _nominationsRetriever.GetNominations(assignNominationsUserContexts, message, votingResults);

            var currentVotingSessionId = context.Message.VotingSessionId;

            var enrichedWinnerEntity = await EnrichWinnerEntityAsync(message.Tenant, votingResults, context.CancellationToken);
            var updateResult = await _votingResultsCommandRepository.UpdateAsync(currentVotingSessionId, votingResults.Movies, nominations, now, moviesAdded, enrichedWinnerEntity, votingResults.MoviesGoingByeBye, context.CancellationToken);

            if (updateResult.Error.HasValue)
                await PublishErrorAsync(context, error: updateResult.Error);

            _logger.LogInformation($"Consumed {nameof(VotingConcludedEvent)} event.");
        }
        catch (Exception ex)
        {
            await PublishErrorAsync(context, ex);
        }
    }

    private Task PublishErrorAsync(ConsumeContext<VotingConcludedEvent> context, Exception? ex = null, Error<VoidResult>? error = null)
    {
        var msg = "Error occurred during concluding the voting..." + error?.ToString();

        if (ex == null)
            _logger.LogError(msg);
        else
            _logger.LogError(ex, msg);

        var errorEvent = new ErrorEvent(context.Message.VotingSessionId, msg, ex?.StackTrace ?? "Unknown");
        return context.Publish(errorEvent, context.CancellationToken);
    }

    private async Task<EmbeddedMovieWithNominationContext> EnrichWinnerEntityAsync(TenantId tenant, VotingResults votingResults, CancellationToken cancelToken)
    {
        var movieId = new MovieId(votingResults.Winner.id);
        var repo = _repositoriesProvider.GetMovieDomainRepository(tenant);
        var nominatedEvents = await repo.GetMovieNominatedEventsAsync(movieId, cancelToken);
        var mostRecentNominatedAgainEvent = nominatedEvents.MaxBy(x => x.Created);
        var nominatingUser = await _usersQueryRepository.GetUserAsync(x => x.id == mostRecentNominatedAgainEvent!.UserId, cancelToken);
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