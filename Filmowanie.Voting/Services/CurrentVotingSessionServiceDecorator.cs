using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Voting.Interfaces;

namespace Filmowanie.Voting.Services;

// TODO UTs
internal sealed class CurrentVotingSessionServiceDecorator : ICurrentVotingSessionIdAccessor
{
    private readonly IVotingSessionService _votingSessionServiceImplementation;
    private readonly ICurrentVotingSessionCacheService _cacheService;

    public CurrentVotingSessionServiceDecorator(IVotingSessionService votingSessionServiceImplementation, ICurrentVotingSessionCacheService cacheService)
    {
        _votingSessionServiceImplementation = votingSessionServiceImplementation;
        _cacheService = cacheService;
    }

    public async Task<Maybe<VotingSessionId?>> GetCurrentVotingSessionIdAsync(Maybe<DomainUser> maybeCurrentUser, CancellationToken cancelToken)
    {
        var tenant = maybeCurrentUser.Map(x => x.Tenant);
        var cached = _cacheService.Cached(tenant);

        if (cached.Error.HasValue)
            return cached.Error.Value.ChangeResultType<(bool, VotingSessionId?), VotingSessionId?>();

        if (cached.Result.CacheHydrated)
            return cached.Result.votingId.AsMaybe();

        var result = await _votingSessionServiceImplementation.GetCurrentVotingSessionIdAsync(maybeCurrentUser, cancelToken);

        if (!result.Error.HasValue && result.Result!.Value.CorrelationId != default)
            _cacheService.Cache(tenant, result);
        
        return result;
    }

    public Maybe<VotingSessionId> GetRequiredVotingSessionId(Maybe<VotingSessionId?> maybeCurrentVotingSessionId) => _votingSessionServiceImplementation.GetRequiredVotingSessionId(maybeCurrentVotingSessionId);

    public Task<Maybe<VotingSessionId>> GetLastVotingSessionIdAsync(Maybe<DomainUser> maybeCurrentUser, CancellationToken cancelToken) => _votingSessionServiceImplementation.GetLastVotingSessionIdAsync(maybeCurrentUser, cancelToken);
}