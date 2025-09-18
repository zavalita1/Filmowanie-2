using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Services;

// TODO UTs
internal sealed class VotingSessionStateManager : IVotingStateManager
{
    private readonly ILogger<VotingSessionStateManager> _log;
    private readonly IBus _bus;
    private readonly IVotingResultsRepository _votingSessionQueryRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IGuidProvider _guidProvider;
    private readonly ICurrentVotingSessionCacheService _currentVotingSessionCacheService;

    public VotingSessionStateManager(ILogger<VotingSessionStateManager> log, IBus bus, IVotingResultsRepository votingSessionQueryRepository, IDateTimeProvider dateTimeProvider, IGuidProvider guidProvider, ICurrentVotingSessionCacheService currentVotingSessionCacheService)
    {
        _log = log;
        _bus = bus;
        _votingSessionQueryRepository = votingSessionQueryRepository;
        _dateTimeProvider = dateTimeProvider;
        _guidProvider = guidProvider;
        _currentVotingSessionCacheService = currentVotingSessionCacheService;
    }

    public Task<Maybe<VoidResult>> ConcludeVotingAsync(Maybe<VotingSessionId> maybeVotingId, Maybe<DomainUser> maybeCurrentUser, CancellationToken cancelToken) =>
        maybeVotingId.Merge(maybeCurrentUser).AcceptAsync(ConcludeVotingAsync, _log, cancelToken);

    public Task<Maybe<VotingSessionId>> StartNewVotingAsync(Maybe<DomainUser> input, CancellationToken cancelToken) =>
        input.AcceptAsync(StartNewVotingAsync, _log, cancelToken);

    public Task<Maybe<VoidResult>> ResumeVotingAsync(Maybe<VotingSessionId> input, CancellationToken cancelToken) => input.AcceptAsync(ResumeVotingAsync, _log, cancelToken);

    private async Task<Maybe<VoidResult>> ResumeVotingAsync(VotingSessionId votingSessionId, CancellationToken cancelToken)
    {
        var message = new ResumeVotingEvent(votingSessionId);
        await _bus.Publish(message, cancelToken);
        return VoidResult.Void;
    }

    private async Task<Maybe<VotingSessionId>> StartNewVotingAsync(DomainUser input, CancellationToken cancelToken)
    {
        var lastConcludedVotingSession = await _votingSessionQueryRepository
            .GetLastNVotingResultsAsync(1, cancelToken);
        var correlationId = _guidProvider.NewGuid();

        if (lastConcludedVotingSession.Error.HasValue)
            return lastConcludedVotingSession.Error.Value.ChangeResultType<IEnumerable<IReadOnlyVotingResult>, VotingSessionId>();

        if (!lastConcludedVotingSession.Result!.Any())
            return new Error<VotingSessionId>("Previous voting has not concluded!", ErrorType.InvalidState);

        var lastVotingResult = lastConcludedVotingSession.Result!.Single();

        if (!Guid.TryParse(lastVotingResult.id, out var lastVotingId))
            return new Error<VotingSessionId>("Can't parse last voting id!", ErrorType.InvalidState);

        var lastVotingResultId = new VotingSessionId(lastVotingId);
        await _bus.Publish(new ResultsConfirmedEvent(lastVotingResultId, input.Tenant), cancelToken);
        
        
        var moviesGoingByeByeIds = lastVotingResult.MoviesGoingByeBye.Select(x => x.id).ToArray();
        var movies = lastVotingResult.Movies.Select(x => new EmbeddedMovie { id = x.Movie.id, Name = x.Movie.Name, MovieCreationYear = x.Movie.MovieCreationYear }).Where(x => x.id != lastVotingResult.Winner.Movie.id).Where(x => !moviesGoingByeByeIds.Contains(x.id)).ToArray();
        var nominationsData = lastVotingResult.UsersAwardedWithNominations.Select(x => new NominationData
        {
            Concluded = null,
            User = new NominationDataEmbeddedUser { DisplayName = x.User.Name, Id = x.User.id },
            Year = x.Decade
        }).ToArray();

        var votingSessionId = new VotingSessionId(correlationId);
        _currentVotingSessionCacheService.InvalidateCache(input.Tenant);
        var @event = new StartVotingEvent(votingSessionId, movies, nominationsData, _dateTimeProvider.Now, input.Tenant);
        await _bus.Publish(@event, cancelToken);

        return new Maybe<VotingSessionId>(votingSessionId, null);
    }

    private async Task<Maybe<VoidResult>> ConcludeVotingAsync((VotingSessionId, DomainUser) input, CancellationToken cancelToken)
    {
        var @event = new ConcludeVotingEvent(input.Item1, input.Item2.Tenant);
        await _bus.Publish(@event, cancelToken);
        return VoidResult.Void;
    }

    public ILogger Log => _log;
}