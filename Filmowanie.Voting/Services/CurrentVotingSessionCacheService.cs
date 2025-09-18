using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Voting.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Services;

// TODO UTs
internal sealed class CurrentVotingSessionCacheService : ICurrentVotingSessionCacheService
{
    private readonly ReaderWriterLockSlim _locker = new(LockRecursionPolicy.NoRecursion);
    private readonly ILogger<CurrentVotingSessionCacheService> _log;

    // Was concurrent dictionary without these manual locks, but changed as a start for future changes
    private readonly Dictionary<TenantId, VotingSessionId?> _cachedValues = new ();

    public CurrentVotingSessionCacheService(ILogger<CurrentVotingSessionCacheService> log)
    {
        _log = log;
    }


    public Maybe<(bool CacheHydrated, VotingSessionId? votingId)> Cached(TenantId tenant)
    {
        _locker.EnterReadLock();
        try
        {
            var result = _cachedValues.TryGetValue(tenant, out var cachedValue);
            return (result, cachedValue).AsMaybe();
        }
        catch (Exception ex)
        {
            var msg = "Encountered error during reading cache!";
            _log.LogError(ex, "Encountered error during reading cache!");
            return new Error<(bool, VotingSessionId?)>(msg, ErrorType.Unknown);
        }
        finally
        {
            _locker.ExitReadLock();
        }
    }

    public Maybe<(bool CacheHydrated, VotingSessionId? votingId)> Cached(Maybe<TenantId> tenant) => tenant.Accept(Cached, _log);

    public Maybe<VoidResult> Cache(Maybe<TenantId> tenant, Maybe<VotingSessionId?> votingId) => tenant.Merge(votingId).Accept(Cache, _log);

    public void InvalidateCache(TenantId tenant)
    {
        _locker.EnterWriteLock();
        try
        {
            _cachedValues.Remove(tenant);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Encountered error during invalidating cache!");
            throw;
        }
        finally
        {
            _locker.ExitWriteLock();
        }
    }

    private Maybe<VoidResult> Cache((TenantId Tenant, VotingSessionId? VotingId) input)
    {
        _locker.EnterWriteLock();
        try
        {
            _cachedValues[input.Tenant] = input.VotingId;
        }
        catch (Exception ex)
        {
            var msg = "Encountered error during invalidating cache!";
            _log.LogError(ex, msg);
            return new Error<VoidResult>(msg, ErrorType.Unknown);
        }
        finally
        {
            _locker.ExitWriteLock();
        }

        return VoidResult.Void;
    }
}