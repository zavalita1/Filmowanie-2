using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Interfaces;
using Filmowanie.Voting.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Visitors;

internal sealed class StartNewVotingVisitor : IStartNewVotingVisitor
{
    private readonly IVotingSessionQueryRepository _votingSessionQueryRepository;
    private readonly IBus _bus;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IGuidProvider _guidProvider;

    private readonly ILogger<StartNewVotingVisitor> _log;

    public StartNewVotingVisitor(IVotingSessionQueryRepository votingSessionQueryRepository, IBus bus, IDateTimeProvider dateTimeProvider, IGuidProvider guidProvider, ILogger<StartNewVotingVisitor> log)
    {
        _votingSessionQueryRepository = votingSessionQueryRepository;
        _bus = bus;
        _dateTimeProvider = dateTimeProvider;
        _guidProvider = guidProvider;
        _log = log;
    }

    public async Task<OperationResult<VotingSessionId>> SignUp(OperationResult<DomainUser> input, CancellationToken cancellationToken)
    {
        var votingSessions = await _votingSessionQueryRepository
            .Get(x => x.Concluded != null, input.Result.Tenant, x => x.Concluded!, -1, cancellationToken);
        var correlationId = _guidProvider.NewGuid();

        if (!votingSessions.Any())
            return new OperationResult<VotingSessionId>(default, new Error("Previous voting has not concluded!", ErrorType.InvalidState));

        var lastVotingResult = votingSessions.Single();
        var movies = lastVotingResult.Movies.Select(x => new EmbeddedMovie { id = x.Movie.id, Name = x.Movie.Name }).Where(x => x.id != lastVotingResult.Winner.id).ToArray();
        var nominationsData = lastVotingResult.UsersAwardedWithNominations.Select(x => new NominationData
        {
            Concluded = null,
            User = new NominationDataEmbeddedUser { DisplayName = x.User.Name, Id = x.User.id },
            Year = x.Decade
        }).ToArray();

        var @event = new StartVotingEvent(correlationId, movies, nominationsData, _dateTimeProvider.Now, input.Result.Tenant);
        await _bus.Publish(@event, cancellationToken);

        var votingSessionId = new VotingSessionId(correlationId);
        return new OperationResult<VotingSessionId>(votingSessionId, null);
    }

    public ILogger Log => _log;
}