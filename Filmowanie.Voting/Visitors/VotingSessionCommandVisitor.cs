using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Interfaces;
using Filmowanie.Voting.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Visitors;

public sealed class VotingSessionCommandVisitor : IStartNewVotingVisitor, IConcludeVotingVisitor
{
    private readonly IVotingSessionQueryRepository _votingSessionQueryRepository;
    private readonly IBus _bus;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<VotingSessionCommandVisitor> _log;

    public VotingSessionCommandVisitor(IVotingSessionQueryRepository votingSessionQueryRepository, IBus bus, IDateTimeProvider dateTimeProvider, ILogger<VotingSessionCommandVisitor> log)
    {
        _votingSessionQueryRepository = votingSessionQueryRepository;
        _bus = bus;
        _dateTimeProvider = dateTimeProvider;
        _log = log;
    }

    public async Task<OperationResult<VotingSessionId>> VisitAsync(OperationResult<DomainUser> input, CancellationToken cancellationToken)
    {
        var votingSessions = await _votingSessionQueryRepository
            .Get(x => x.TenantId == input.Result.Tenant.Id && x.Concluded != null, x => x.Concluded!, -1, cancellationToken);
        var correlationId = Guid.NewGuid();

        if (!votingSessions.Any())
            return new OperationResult<VotingSessionId>(default, new Error("Previous voting has not concluded!", ErrorType.InvalidState));

        var lastVotingResult = votingSessions.Single();
        var movies = lastVotingResult.Movies.Select(x => new EmbeddedMovie { id = x.Movie.id, Name = x.Movie.Name }).Where(x => x.id != lastVotingResult.Winner.id).ToArray();
        var nominationsData = lastVotingResult.UsersAwardedWithNominations.Select(x => new NominationData
        {
            Concluded = null,
            User = new NominationDataEmbeddedUser { DisplayName = x.User.Name, Id = x.User.id}
        }).ToArray();

        var @event = new StartVotingEvent(correlationId, movies, nominationsData, _dateTimeProvider.Now, input.Result.Tenant);
        await _bus.Publish(@event, cancellationToken);

        var votingSessionId = new VotingSessionId(correlationId);
        return new OperationResult<VotingSessionId>(votingSessionId, null);
    }

    public async Task<OperationResult<object>> VisitAsync(OperationResult<(VotingSessionId, DomainUser)> input, CancellationToken cancellationToken)
    {
        var @event = new ConcludeVotingEvent(input.Result.Item1.CorrelationId, input.Result.Item2.Tenant);
        await _bus.Publish(@event, cancellationToken);
        return OperationResultExtensions.Empty;
    }

    public ILogger Log => _log;
}