using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Maybe;

namespace Filmowanie.Abstractions.Interfaces;

public interface ICurrentVotingSessionIdAccessor
{
    Task<Maybe<VotingSessionId?>> GetCurrentVotingSessionIdAsync(Maybe<DomainUser> maybeCurrentUser, CancellationToken cancelToken);
    Maybe<VotingSessionId> GetRequiredVotingSessionId(Maybe<VotingSessionId?> maybeCurrentVotingSessionId);

}