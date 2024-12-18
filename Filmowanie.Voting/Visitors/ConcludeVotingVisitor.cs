using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Voting.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Visitors;

internal sealed class ConcludeVotingVisitor : IConcludeVotingVisitor
{
    private readonly ILogger<ConcludeVotingVisitor> _log;
    private readonly IBus _bus;

    public ConcludeVotingVisitor(ILogger<ConcludeVotingVisitor> log, IBus bus)
    {
        _log = log;
        _bus = bus;
    }

    public async Task<OperationResult<object>> VisitAsync(OperationResult<(VotingSessionId, DomainUser)> input, CancellationToken cancellationToken)
    {
        var @event = new ConcludeVotingEvent(input.Result.Item1.CorrelationId, input.Result.Item2.Tenant);
        await _bus.Publish(@event, cancellationToken);
        return OperationResultExtensions.Empty;
    }

    public ILogger Log => _log;
}