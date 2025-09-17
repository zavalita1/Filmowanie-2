using System.Linq.Expressions;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Interfaces;

internal interface IMovieQueryRepository
{
    public Task<IReadOnlyMovieEntity[]> GetMoviesAsync(Expression<Func<IReadOnlyMovieEntity, bool>> predicate, CancellationToken cancelToken);
    public Task<IReadOnlyCanNominateMovieAgainEvent[]> GetMoviesThatCanBeNominatedAgainEventsAsync(Expression<Func<IReadOnlyCanNominateMovieAgainEvent, bool>> predicate,
        CancellationToken cancelToken);
    public Task<IReadOnlyNominatedMovieEvent[]> GetMovieNominatedEventsAsync(Expression<Func<IReadOnlyNominatedMovieEvent, bool>> predicate,
        CancellationToken cancelToken);
}