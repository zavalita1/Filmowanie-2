using Filmowanie.Database.Contexts;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Filmowanie.Database.Repositories;

public interface IMovieQueryRepository
{
    Task<IReadOnlyMovieEntity[]> GetMoviesAsync(Expression<Func<IReadOnlyMovieEntity, bool>> predicate, CancellationToken cancellationToken);
}

internal sealed class MovieQueryRepository : IMovieQueryRepository
{
    private readonly MoviesContext _ctx;

    public MovieQueryRepository(MoviesContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<IReadOnlyMovieEntity[]> GetMoviesAsync(Expression<Func<IReadOnlyMovieEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _ctx.Movies.Where(predicate).ToArrayAsync(cancellationToken);
    }
}