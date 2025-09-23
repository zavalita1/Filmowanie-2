using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Voting.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Services;

// TODO UTs
internal sealed class CurrentVotingSessionCacheService : ICurrentVotingSessionCacheService
{
    private readonly ReaderWriterLockSlim locker = new(LockRecursionPolicy.NoRecursion);
    private readonly ILogger<CurrentVotingSessionCacheService> log;

    // Was concurrent dictionary without these manual locks, but changed as a start for future changes
    private readonly Dictionary<TenantId, VotingSessionId?> cachedValues = new ();

    public CurrentVotingSessionCacheService(ILogger<CurrentVotingSessionCacheService> log)
    {
        this.log = log;
    }

    public Maybe<(bool CacheHydrated, VotingSessionId? votingId)> Cached(TenantId tenant)
    {
        this.locker.EnterReadLock();
        try
        {
            var result = this.cachedValues.TryGetValue(tenant, out var cachedValue);
            return (result, cachedValue).AsMaybe();
        }
        catch (Exception ex)
        {
            var msg = "Encountered error during reading cache!";
            this.log.LogError(ex, "Encountered error during reading cache!");
            return new Error<(bool, VotingSessionId?)>(msg, ErrorType.Unknown);
        }
        finally
        {
            this.locker.ExitReadLock();
        }
    }

    public Maybe<(bool CacheHydrated, VotingSessionId? votingId)> Cached(Maybe<TenantId> tenant) => tenant.Accept(Cached, this.log);

    public Maybe<VoidResult> Cache(Maybe<TenantId> tenant, Maybe<VotingSessionId?> votingId) => tenant.Merge(votingId).Accept(Cache, this.log);

    public void InvalidateCache(TenantId tenant)
    {
        this.locker.EnterWriteLock();
        try
        {
            this.cachedValues.Remove(tenant);
        }
        catch (Exception ex)
        {
            this.log.LogError(ex, "Encountered error during invalidating cache!");
            throw;
        }
        finally
        {
            this.locker.ExitWriteLock();
        }
    }

    private Maybe<VoidResult> Cache((TenantId Tenant, VotingSessionId? VotingId) input)
    {
        this.locker.EnterWriteLock();
        try
        {
            this.cachedValues[input.Tenant] = input.VotingId;
        }
        catch (Exception ex)
        {
            var msg = "Encountered error during invalidating cache!";
            this.log.LogError(ex, msg);
            return new Error<VoidResult>(msg, ErrorType.Unknown);
        }
        finally
        {
            this.locker.ExitWriteLock();
        }

        return VoidResult.Void;
    }
}