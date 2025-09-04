using Filmowanie.Abstractions.OperationResult;

namespace Filmowanie.Abstractions.Interfaces;

public interface ICurrentVotingSessionIdAccessor
{
    Task<Maybe<VotingSessionId?>> GetCurrentVotingSessionIdAsync(Maybe<DomainUser> maybeCurrentUser, CancellationToken cancellationToken);
    Maybe<VotingSessionId> GetRequiredVotingSessionId(Maybe<VotingSessionId?> maybeCurrentVotingSessionId);

}