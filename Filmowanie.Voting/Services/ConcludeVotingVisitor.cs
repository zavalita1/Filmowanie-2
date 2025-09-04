using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Database.Interfaces;
using Filmowanie.Voting.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Services;

internal sealed class VotingSessionStateManager : IVotingStateManager
{
    private readonly ILogger<VotingSessionStateManager> _log;
    private readonly IBus _bus;
    private readonly IVotingSessionQueryRepository _votingSessionQueryRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IGuidProvider _guidProvider;

    public VotingSessionStateManager(ILogger<VotingSessionStateManager> log, IBus bus, IVotingSessionQueryRepository votingSessionQueryRepository, IDateTimeProvider dateTimeProvider, IGuidProvider guidProvider)
    {
        _log = log;
        _bus = bus;
        _votingSessionQueryRepository = votingSessionQueryRepository;
        _dateTimeProvider = dateTimeProvider;
        _guidProvider = guidProvider;
    }

    public Task<Maybe<VoidResult>> ConcludeVotingAsync(Maybe<(VotingSessionId, DomainUser)> input, CancellationToken cancellationToken) =>
        input.AcceptAsync(ConcludeVotingAsync, _log, cancellationToken);

    public Task<Maybe<VotingSessionId>> StartNewVotingAsync(Maybe<DomainUser> input, CancellationToken cancellationToken) =>
        input.AcceptAsync(StartNewVotingAsync, _log, cancellationToken);

    private async Task<Maybe<VotingSessionId>> StartNewVotingAsync(DomainUser input, CancellationToken cancellationToken)
    {
        var votingSessions = await _votingSessionQueryRepository
            .Get(x => x.Concluded != null, input.Tenant, x => x.Concluded!, -1, cancellationToken);
        var correlationId = _guidProvider.NewGuid();

        if (!votingSessions.Any())
            return new Error("Previous voting has not concluded!", ErrorType.InvalidState).AsMaybe<VotingSessionId>();

        var lastVotingResult = votingSessions.Single();
        var movies = lastVotingResult.Movies.Select(x => new EmbeddedMovie { id = x.Movie.id, Name = x.Movie.Name }).Where(x => x.id != lastVotingResult.Winner.id).ToArray();
        var nominationsData = lastVotingResult.UsersAwardedWithNominations.Select(x => new NominationData
        {
            Concluded = null,
            User = new NominationDataEmbeddedUser { DisplayName = x.User.Name, Id = x.User.id },
            Year = x.Decade
        }).ToArray();

        var votingSessionId = new VotingSessionId(correlationId);
        var @event = new StartVotingEvent(votingSessionId, movies, nominationsData, _dateTimeProvider.Now, input.Tenant);
        await _bus.Publish(@event, cancellationToken);

        return new Maybe<VotingSessionId>(votingSessionId, null);
    }

    private async Task<Maybe<VoidResult>> ConcludeVotingAsync((VotingSessionId, DomainUser) input, CancellationToken cancellationToken)
    {
        var @event = new ConcludeVotingEvent(input.Item1, input.Item2.Tenant);
        await _bus.Publish(@event, cancellationToken);
        return VoidResult.Void;
    }

    public ILogger Log => _log;
}