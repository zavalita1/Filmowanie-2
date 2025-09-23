using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Database.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Database.Repositories;

internal sealed class RepositoryInUserlessContextProvider : IRepositoryInUserlessContextProvider
{
    private readonly IVotingSessionQueryRepositoryInUserlessContextProvider resultsProvider;
    private readonly IMovieQueryRepositoryInUserslessContextProvider moviesProvider;
    private readonly ILoggerFactory loggerFactory;

    public RepositoryInUserlessContextProvider(
        IVotingSessionQueryRepositoryInUserlessContextProvider resultsProvider,
        IMovieQueryRepositoryInUserslessContextProvider moviesProvider,
        ILoggerFactory loggerFactory)
    {
        this.resultsProvider = resultsProvider;
        this.moviesProvider = moviesProvider;
        this.loggerFactory = loggerFactory;
    }

    public IMovieDomainRepository GetMovieDomainRepository(TenantId tenant)
    {
        var repo = moviesProvider.GetRepository(tenant);
        var log = this.loggerFactory.CreateLogger<MovieDomainRepository>();
        var result = new MovieDomainRepository(repo, log);
        return result;
    }

    public IVotingResultsRepository GetVotingResultsRepository(TenantId tenant)
    {
        var repo = resultsProvider.GetRepository(tenant);
        var log = this.loggerFactory.CreateLogger<VotingResultsRepository>();
        var result = new VotingResultsRepository(repo, log);
        return result;
    }
}
