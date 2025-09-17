using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Database.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Database.Repositories;

internal sealed class RepositoryInUserlessContextProvider : IRepositoryInUserlessContextProvider
{
    private readonly IVotingSessionQueryRepositoryInUserlessContextProvider _resultsProvider;
    private readonly IMovieQueryRepositoryInUserslessContextProvider _moviesProvider;
    private readonly ILoggerFactory _loggerFactory;

    public RepositoryInUserlessContextProvider(
        IVotingSessionQueryRepositoryInUserlessContextProvider resultsProvider,
        IMovieQueryRepositoryInUserslessContextProvider moviesProvider,
        ILoggerFactory loggerFactory)
    {
        _resultsProvider = resultsProvider;
        _moviesProvider = moviesProvider;
        _loggerFactory = loggerFactory;
    }

    public IMovieDomainRepository GetMovieDomainRepository(TenantId tenant)
    {
        var repo = _moviesProvider.GetRepository(tenant);
        var log = _loggerFactory.CreateLogger<MovieDomainRepository>();
        var result = new MovieDomainRepository(repo, log);
        return result;
    }

    public IVotingResultsRepository GetVotingResultsRepository(TenantId tenant)
    {
        var repo = _resultsProvider.GetRepository(tenant);
        var log = _loggerFactory.CreateLogger<VotingResultsRepository>();
        var result = new VotingResultsRepository(repo, log);
        return result;
    }
}
