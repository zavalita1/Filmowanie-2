using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Maybe;

namespace Filmowanie.Voting.Interfaces;

internal interface ICurrentVotingSessionCacheService
{
    Maybe<(bool CacheHydrated, VotingSessionId? votingId)> Cached(TenantId tenant);
    Maybe<(bool CacheHydrated, VotingSessionId? votingId)> Cached(Maybe<TenantId> tenant);
    Maybe<VoidResult> Cache(Maybe<TenantId> tenant, Maybe<VotingSessionId?> votingId);

    void InvalidateCache(TenantId tenant);
}