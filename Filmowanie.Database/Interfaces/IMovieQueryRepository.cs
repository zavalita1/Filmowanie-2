using System.Linq.Expressions;
using Filmowanie.Abstractions;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Interfaces;

public interface IMovieQueryRepository
{
    public Task<IReadOnlyMovieEntity[]> GetMoviesAsync(Expression<Func<IReadOnlyMovieEntity, bool>> predicate, TenantId tenant, CancellationToken cancellationToken);
    public Task<IReadOnlyCanNominateMovieAgainEvent[]> GetMoviesThatCanBeNominatedAgainEventsAsync(Expression<Func<IReadOnlyCanNominateMovieAgainEvent, bool>> predicate, TenantId tenant,
        CancellationToken cancellationToken);
    public Task<IReadOnlyNominatedMovieAgainEvent[]> GetMoviesNominatedAgainEventsAsync(Expression<Func<IReadOnlyNominatedMovieAgainEvent, bool>> predicate, TenantId tenant,
        CancellationToken cancellationToken);
}