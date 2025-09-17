using Filmowanie.Abstractions.DomainModels;

namespace Filmowanie.Database.Interfaces;

public interface IRepositoryInUserlessContextProvider
{
    IVotingResultsRepository GetVotingResultsRepository(TenantId tenant);
    IMovieDomainRepository GetMovieDomainRepository(TenantId tenant);
}