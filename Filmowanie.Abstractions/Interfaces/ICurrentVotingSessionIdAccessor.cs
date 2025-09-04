using Filmowanie.Abstractions.OperationResult;

namespace Filmowanie.Abstractions.Interfaces;

public interface ICurrentVotingSessionIdAccessor
{
    Task<OperationResult<VotingSessionId?>> GetCurrentVotingSessionId(OperationResult<DomainUser> maybeCurrentUser, CancellationToken cancellationToken);
    OperationResult<VotingSessionId> GetRequiredCurrentVotingSessionId(OperationResult<VotingSessionId?> maybeCurrentVotingSessionId);

}