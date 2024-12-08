using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Database.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Visitors;

internal sealed class VotingSessionIdQueryVisitor : IGetCurrentVotingSessionIdVisitor, IGetCurrentVotingSessionStatusVisitor, IRequireCurrentVotingSessionIdVisitor
{
    private readonly IVotingSessionQueryRepository _votingSessionQueryRepository;
    private readonly ILogger<VotingSessionIdQueryVisitor> _log;

    public VotingSessionIdQueryVisitor(IVotingSessionQueryRepository votingSessionQueryRepository, ILogger<VotingSessionIdQueryVisitor> log)
    {
        _votingSessionQueryRepository = votingSessionQueryRepository;
        _log = log;
    }

    // IGetCurrentVotingSessionVisitor
    public async Task<OperationResult<VotingSessionId?>> VisitAsync(OperationResult<DomainUser> input, CancellationToken cancellationToken)
    {
        var currentVotingResults = await _votingSessionQueryRepository.Get(x => x.TenantId == input.Result.Tenant.Id && x.Concluded == null, cancellationToken);

        if (currentVotingResults == null)
            return new OperationResult<VotingSessionId?>(null, null);

        var correlationId = Guid.Parse(currentVotingResults.Id);
        var votingSessionId = new VotingSessionId(correlationId);
        return new OperationResult<VotingSessionId?>(votingSessionId, null);
    }

    // IGetCurrentVotingSessionStatusVisitor
    async Task<OperationResult<VotingState>> IOperationAsyncVisitor<DomainUser, VotingState>.VisitAsync(OperationResult<DomainUser> input, CancellationToken cancellationToken)
    {
        var sagaId = await _votingSessionQueryRepository.Get(x => x.TenantId == input.Result.Tenant.Id && x.Concluded == null, cancellationToken);
        var state = sagaId == null ? VotingState.Results : VotingState.Voting;
        return new OperationResult<VotingState>(state, null);
    }

    public OperationResult<VotingSessionId> Visit(OperationResult<VotingSessionId?> input)
    {
        if (!input.Result.HasValue)
            return new OperationResult<VotingSessionId>(default, new Error("Voting has not started yet!", ErrorType.IncomingDataIssue));

        return new OperationResult<VotingSessionId>(input.Result.Value, null);
    }

    public ILogger Log => _log;
}
