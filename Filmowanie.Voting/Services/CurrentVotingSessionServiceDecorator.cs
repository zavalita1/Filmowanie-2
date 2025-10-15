using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Voting.Interfaces;

namespace Filmowanie.Voting.Services;

// TODO UTs
internal sealed class CurrentVotingSessionServiceDecorator : ICurrentVotingSessionIdAccessor
{
    private readonly IVotingSessionService votingSessionServiceImplementation;
    private readonly ICurrentVotingSessionCacheService cacheService;

    public CurrentVotingSessionServiceDecorator(IVotingSessionService votingSessionServiceImplementation, ICurrentVotingSessionCacheService cacheService)
    {
        this.votingSessionServiceImplementation = votingSessionServiceImplementation;
        this.cacheService = cacheService;
    }

    public async Task<Maybe<VotingSessionId?>> GetCurrentVotingSessionIdAsync(Maybe<DomainUser> maybeCurrentUser, CancellationToken cancelToken)
    {
        var tenant = maybeCurrentUser.Map(x => x.Tenant);
        var cached = this.cacheService.Cached(tenant);

        if (cached.Error.HasValue)
            return cached.Error.Value.ChangeResultType<(bool, VotingSessionId?), VotingSessionId?>();

        if (cached.Result.CacheHydrated)
            return cached.Result.votingId.AsMaybe();

        var result = await this.votingSessionServiceImplementation.GetCurrentVotingSessionIdAsync(maybeCurrentUser, cancelToken);

        if (!result.Error.HasValue && result.Result?.CorrelationId != default)
            this.cacheService.Cache(tenant, result);
        
        return result;
    }

    public Maybe<VotingSessionId> GetRequiredVotingSessionId(Maybe<VotingSessionId?> maybeCurrentVotingSessionId) => this.votingSessionServiceImplementation.GetRequiredVotingSessionId(maybeCurrentVotingSessionId);

    public Task<Maybe<VotingSessionId>> GetLastVotingSessionIdAsync(Maybe<DomainUser> maybeCurrentUser, CancellationToken cancelToken) => this.votingSessionServiceImplementation.GetLastVotingSessionIdAsync(maybeCurrentUser, cancelToken);
}