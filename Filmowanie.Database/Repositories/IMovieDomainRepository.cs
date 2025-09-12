using Filmowanie.Abstractions;
using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Repositories;

public interface IMovieDomainRepository
{
    Task<Maybe<IReadOnlyMovieEntity[]>> GetManyByIdAsync(IEnumerable<string> ids, CancellationToken cancelToken, bool requireExactCountsMatch = true);
    Task<Maybe<IReadOnlyMovieEntity>> GetByIdAsync(string id, CancellationToken cancelToken);
    Task<Maybe<IReadOnlyMovieEntity?>> GetByNameAsync(string name, int creationYear, CancellationToken cancelToken);

    public Task<IReadOnlyCanNominateMovieAgainEvent[]> GetMoviesThatCanBeNominatedAgainEventsAsync(CancellationToken cancelToken);
    public Task<IReadOnlyNominatedMovieEvent[]> GetMovieNominatedEventsAsync(CancellationToken cancelToken);
    public Task<IReadOnlyNominatedMovieEvent[]> GetMovieNominatedEventsAsync(string movieName, int movieCreationYear, CancellationToken cancelToken);

    public Task<IReadOnlyNominatedMovieEvent[]> GetMovieNominatedEventsAsync(MovieId movieId, CancellationToken cancelToken);
}