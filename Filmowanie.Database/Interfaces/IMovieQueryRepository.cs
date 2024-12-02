using System.Linq.Expressions;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Interfaces;

public interface IMovieQueryRepository
{
    public Task<IReadOnlyMovieEntity[]> GetMoviesAsync(Expression<Func<IReadOnlyMovieEntity, bool>> predicate, CancellationToken cancellationToken);
    public Task<IReadOnlyMoviesThatCanBeNominatedAgainEntity?> GetMoviesThatCanBeNominatedAgainEntityAsync(Expression<Func<IReadOnlyMoviesThatCanBeNominatedAgainEntity, bool>> predicate, CancellationToken cancellationToken);
}