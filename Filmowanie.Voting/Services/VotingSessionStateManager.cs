using Filmowanie.Abstractions;
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

    public VotingSessionStateManager(ILogger<VotingSessionStateManager> log, IBus bus, IVotingResultsRepository votingSessionQueryRepository, IDateTimeProvider dateTimeProvider, IGuidProvider guidProvider)
    {
        _log = log;
        _bus = bus;
        _votingSessionQueryRepository = votingSessionQueryRepository;
        _dateTimeProvider = dateTimeProvider;
        _guidProvider = guidProvider;
    }

    public Task<Maybe<VoidResult>> ConcludeVotingAsync(Maybe<(VotingSessionId, DomainUser)> input, CancellationToken cancelToken) =>
        input.AcceptAsync(ConcludeVotingAsync, _log, cancelToken);

    public Task<Maybe<VotingSessionId>> StartNewVotingAsync(Maybe<DomainUser> input, CancellationToken cancelToken) =>
        input.AcceptAsync(StartNewVotingAsync, _log, cancelToken);

    private async Task<Maybe<VotingSessionId>> StartNewVotingAsync(DomainUser input, CancellationToken cancelToken)
    {
        // TODO error here
        var lastConcludedVotingSession = await _votingSessionQueryRepository
            .GetLastNVotingResultsAsync(1, cancelToken);
        var correlationId = _guidProvider.NewGuid();

        if (lastConcludedVotingSession.Error.HasValue)
            return lastConcludedVotingSession.Error.Value.ChangeResultType<IEnumerable<IReadOnlyVotingResult>, VotingSessionId>();

        if (!lastConcludedVotingSession.Result!.Any())
            return new Error<VotingSessionId>("Previous voting has not concluded!", ErrorType.InvalidState);

        var lastVotingResult = lastConcludedVotingSession.Result!.Single();
        var moviesGoingByeByeIds = lastVotingResult.MoviesGoingByeBye.Select(x => x.id).ToArray();
        var movies = lastVotingResult.Movies.Select(x => new EmbeddedMovie { id = x.Movie.id, Name = x.Movie.Name }).Where(x => x.id != lastVotingResult.Winner.Movie.id).Where(x => !moviesGoingByeByeIds.Contains(x.id)).ToArray();
        var nominationsData = lastVotingResult.UsersAwardedWithNominations.Select(x => new NominationData
        {
            Concluded = null,
            User = new NominationDataEmbeddedUser { DisplayName = x.User.Name, Id = x.User.id },
            Year = x.Decade
        }).ToArray();

        var votingSessionId = new VotingSessionId(correlationId);
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