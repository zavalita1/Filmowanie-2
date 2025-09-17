using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Database.Repositories;

internal sealed class MovieDomainRepository : IMovieDomainRepository
{
    private readonly IMovieQueryRepository _movieQueryRepository;
    private readonly ILogger<MovieDomainRepository> _logger;

    public MovieDomainRepository(IMovieQueryRepository movieQueryRepository, ILogger<MovieDomainRepository> logger)
    {
        _movieQueryRepository = movieQueryRepository;
        _logger = logger;
    }

    public async Task<Maybe<IReadOnlyMovieEntity[]>> GetManyByIdAsync(IEnumerable<string> ids, CancellationToken cancelToken, bool requireExactCountsMatch = true)
    {
        try
        {
            var result = await _movieQueryRepository.GetMoviesAsync(x => ids.Contains(x.id), cancelToken);

            if (requireExactCountsMatch && result.Length != ids.Count())
                return new Error<IReadOnlyMovieEntity[]>("Did not found all requested movies!", ErrorType.IncomingDataIssue);

            return result.AsMaybe();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during fetching movie data from db!");
            return new Error<IReadOnlyMovieEntity[]>(ex.Message, ErrorType.Unknown);
        }
    } 

    public async Task<Maybe<IReadOnlyMovieEntity>> GetByIdAsync(string id, CancellationToken cancelToken)
    {
        try
        {
            var result = await _movieQueryRepository.GetMoviesAsync(x => x.id == id, cancelToken);
            if (!result.Any())
                return new Error<IReadOnlyMovieEntity>("No such movie found!", ErrorType.IncomingDataIssue);

            return result.Single().AsMaybe();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during fetching movie data from db!");
            return new Error<IReadOnlyMovieEntity>(ex.Message, ErrorType.Unknown);
        }
    }

    public async Task<Maybe<IReadOnlyMovieEntity?>> GetByNameAsync(string name, int creationYear, CancellationToken cancelToken)
    {
        try
        {
            var result = await _movieQueryRepository.GetMoviesAsync(x => x.Name == name && x.CreationYear == creationYear, cancelToken);
            return result.SingleOrDefault().AsMaybe();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during fetching movie data from db!");
            return new Error<IReadOnlyMovieEntity?>(ex.Message, ErrorType.Unknown);
        }
    }

    public Task<IReadOnlyCanNominateMovieAgainEvent[]> GetMoviesThatCanBeNominatedAgainEventsAsync(CancellationToken cancelToken)
    {
        return _movieQueryRepository.GetMoviesThatCanBeNominatedAgainEventsAsync(_ => true, cancelToken);
    }

    public Task<IReadOnlyNominatedMovieEvent[]> GetMovieNominatedEventsAsync(CancellationToken cancelToken)
    {
        return _movieQueryRepository.GetMovieNominatedEventsAsync(_ => true, cancelToken);
    }

    public Task<IReadOnlyNominatedMovieEvent[]> GetMovieNominatedEventsAsync(string movieName, int movieCreationYear, CancellationToken cancelToken)
    {
        return _movieQueryRepository.GetMovieNominatedEventsAsync(x => x.MovieName == movieName && x.MovieCreationYear == movieCreationYear, cancelToken);
    }

    public Task<IReadOnlyNominatedMovieEvent[]> GetMovieNominatedEventsAsync(MovieId movieId, CancellationToken cancelToken)
    {
        return _movieQueryRepository.GetMovieNominatedEventsAsync(x => x.MovieId == movieId.Id, cancelToken);
    }
}