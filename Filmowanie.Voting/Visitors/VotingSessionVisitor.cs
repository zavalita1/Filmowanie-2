using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Database.Interfaces;
using Filmowanie.Voting.Interfaces;
using VotingSessionId = Filmowanie.Abstractions.VotingSessionId;

namespace Filmowanie.Voting.Visitors;

internal sealed class VotingSessionQueryVisitor : IGetCurrentVotingSessionVisitor, IGetCurrentVotingSessionStatusVisitor
{
    private readonly IVotingSessionQueryRepository _votingSessionQueryRepository;

    public VotingSessionQueryVisitor(IVotingSessionQueryRepository votingSessionQueryRepository)
    {
        _votingSessionQueryRepository = votingSessionQueryRepository;
    }

    public async Task<OperationResult<VotingSessionId>> VisitAsync(OperationResult<DomainUser> input, CancellationToken cancellationToken)
    {
        var currentVotingResults = await _votingSessionQueryRepository.GetCurrent(input.Result.Tenant, cancellationToken);

        if (currentVotingResults == null)
            return new OperationResult<VotingSessionId>(default, new Error("Voting has not started!", ErrorType.InvalidState));

        var correlationId = Guid.Parse(currentVotingResults.Id);
        var votingSessionId = new VotingSessionId(correlationId);
        return new OperationResult<VotingSessionId>(votingSessionId, null);
    }

    async Task<OperationResult<VotingState>> IOperationAsyncVisitor<DomainUser, VotingState>.VisitAsync(OperationResult<DomainUser> input, CancellationToken cancellationToken)
    {
        var sagaId = await _votingSessionQueryRepository.GetCurrent(input.Result.Tenant, cancellationToken);
        var state = sagaId == null ? VotingState.Results : VotingState.Voting;
        return new OperationResult<VotingState>(state, null);
    }
}