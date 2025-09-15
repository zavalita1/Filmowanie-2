using Filmowanie.Abstractions;
using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Database.Repositories;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Consumers;

// TODO UTs
internal sealed class VotingConcludedConsumer : IConsumer<VotingConcludedEvent>, IConsumer<Fault<VotingConcludedEvent>>
{
    private readonly ILogger<VotingConcludedConsumer> _logger;
    private readonly IVotingResultsCommandRepository _votingResultsCommandRepository;
    private readonly IVotingResultsRepository _votingResultsRepository;
    private readonly IMovieDomainRepository _movieDomainRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPickUserToNominateContextRetriever _pickUserToNominateContextRetriever;
    private readonly IVotingResultsRetriever _votingResultsRetriever;
    private readonly INominationsRetriever _nominationsRetriever;
    private readonly IUsersQueryRepository _usersQueryRepository;

    private const int DecidersTimeWindow = 10;

    public VotingConcludedConsumer(ILogger<VotingConcludedConsumer> logger, IVotingResultsCommandRepository votingResultsCommandRepository, IDateTimeProvider dateTimeProvider, IVotingResultsRepository votingResultsRepository, IPickUserToNominateContextRetriever pickUserToNominateContextRetriever, IVotingResultsRetriever votingResultsRetriever, INominationsRetriever nominationsRetriever, IMovieDomainRepository movieDomainRepository, IUsersQueryRepository usersQueryRepository)
    {
        _logger = logger;
        _votingResultsCommandRepository = votingResultsCommandRepository;
        _dateTimeProvider = dateTimeProvider;
        _votingResultsRepository = votingResultsRepository;
        _pickUserToNominateContextRetriever = pickUserToNominateContextRetriever;
        _votingResultsRetriever = votingResultsRetriever;
        _nominationsRetriever = nominationsRetriever;
        _movieDomainRepository = movieDomainRepository;
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
        var now = _dateTimeProvider.Now;
        var message = context.Message;

        var lastFewVotingResults = (await _votingResultsRepository.GetLastNVotingResultsAsync(DecidersTimeWindow, context.CancellationToken)).RequireResult().ToArray();
        var lastVotingResult = lastFewVotingResults.FirstOrDefault(); // null only if it's initial voting
        var votingResults = _votingResultsRetriever.GetVotingResults(message.MoviesWithVotes, lastVotingResult); 

        var moviesAdded = GetMoviesAddedDuringSession(message);

        var assignNominationsUserContexts = _pickUserToNominateContextRetriever.GetPickUserToNominateContexts(lastFewVotingResults, moviesAdded, message);
        var nominations = _nominationsRetriever.GetNominations(assignNominationsUserContexts, message, votingResults);

        var currentVotingSessionId = context.Message.VotingSessionId.CorrelationId.ToString();

        var enrichedWinnerEntity = await EnrichWinnerEntityAsync(votingResults, context.CancellationToken); 
        await _votingResultsCommandRepository.UpdateAsync(currentVotingSessionId, votingResults.Movies, nominations, now, moviesAdded, enrichedWinnerEntity, context.CancellationToken);
        await context.Publish(new ResultsCalculatedEvent(message.VotingSessionId));

        _logger.LogInformation($"Consumed {nameof(VotingConcludedEvent)} event.");
    }

    private async Task<EmbeddedMovieWithNominationContext> EnrichWinnerEntityAsync(VotingResults votingResults, CancellationToken cancelToken)
    {
        var movieId = new MovieId(votingResults.Winner.id);
        var nominatedEvents = await _movieDomainRepository.GetMovieNominatedEventsAsync(movieId, cancelToken);
        var mostRecentNominatedAgainEvent = nominatedEvents.MaxBy(x => x.Created);
        var user = await _usersQueryRepository.GetUserAsync(x => x.id == mostRecentNominatedAgainEvent!.UserId, cancelToken);
        return new EmbeddedMovieWithNominationContext(votingResults.Winner) { NominatedBy = new EmbeddedUser(user!)};
    }

    private static IReadOnlyEmbeddedMovieWithNominationContext[] GetMoviesAddedDuringSession(VotingConcludedEvent message)
    {
        return message.NominationsData.Join(message.MoviesWithVotes, x => x.MovieId, x => x.Movie.id, IReadOnlyEmbeddedMovieWithNominationContext (x, y) => new ReadOnlyEmbeddedMovieWithNominationContext
        (
            y.Movie,
            new EmbeddedUser { id = x.User.Id, Name = x.User.DisplayName, TenantId = message.Tenant.Id },
            x.Concluded!.Value,
            message.VotingStarted
        )).ToArray();
    }

    private readonly record struct ReadOnlyEmbeddedMovieWithNominationContext(IReadOnlyEmbeddedMovie Movie, IReadOnlyEmbeddedUser NominatedBy, DateTime NominationConcluded, DateTime NominationStarted)
        : IReadOnlyEmbeddedMovieWithNominationContext;
}