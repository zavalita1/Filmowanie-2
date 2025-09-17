using Filmowanie.Abstractions.DomainModels;

namespace Filmowanie.Database.Interfaces;

internal interface IMovieQueryRepositoryInUserslessContextProvider
{
    IMovieQueryRepository GetRepository(TenantId tenantId);
}