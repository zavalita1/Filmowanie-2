using Filmowanie.Abstractions.DomainModels;

namespace Filmowanie.Database.Interfaces;

internal interface IVotingSessionQueryRepositoryInUserlessContextProvider
{
    IVotingSessionQueryRepositoryInUserlessContext GetRepository(TenantId tenantId);
}
