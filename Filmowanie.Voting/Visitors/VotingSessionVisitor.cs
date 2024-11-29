using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Database.Interfaces;
using Filmowanie.Voting.Interfaces;

namespace Filmowanie.Voting.Visitors;

internal sealed class VotingSessionVisitor : IGetCurrentVotingSessionVisitor, IGetCurrentVotingSessionStatusVisitor
{
    private readonly IVotingSessionQueryRepository _votingSessionQueryRepository;

    public VotingSessionVisitor(IVotingSessionQueryRepository votingSessionQueryRepository)
    {
        _votingSessionQueryRepository = votingSessionQueryRepository;
    }

    public async Task<OperationResult<VotingSessionId>> VisitAsync(OperationResult<DomainUser> input, CancellationToken cancellationToken)
    {
        var sagaId = await _votingSessionQueryRepository.GetStartedEventsAsync(input.Result.Tenant, cancellationToken);

        if (sagaId == null)
            return new OperationResult<VotingSessionId>(default, new Error("Voting has not started!", ErrorType.InvalidState));

        return new OperationResult<VotingSessionId>(sagaId.Value, null);
    }

    async Task<OperationResult<VotingState>> IOperationAsyncVisitor<DomainUser, VotingState>.VisitAsync(OperationResult<DomainUser> input, CancellationToken cancellationToken)
    {
        var sagaId = await _votingSessionQueryRepository.GetStartedEventsAsync(input.Result.Tenant, cancellationToken);
        var state = sagaId == null ? VotingState.Results : VotingState.Voting;
        return new OperationResult<VotingState>(state, null);
    }
}