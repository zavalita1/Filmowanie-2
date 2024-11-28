using Filmowanie.Abstractions;

namespace Filmowanie.Database.Interfaces;

public interface IVotingSessionQueryRepository
{
    public Task<VotingSessionId> GetStartedEventsAsync(TenantId tenantId, CancellationToken cancellationToken);
}