using Filmowanie.Database.Contexts;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using System.Linq.Expressions;
using Filmowanie.Database.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Filmowanie.Database.Repositories;

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

    public async Task<IReadOnlyMoviesThatCanBeNominatedAgainEntity?> GetMoviesThatCanBeNominatedAgainEntityAsync(Expression<Func<IReadOnlyMoviesThatCanBeNominatedAgainEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _ctx.MoviesThatCanBeNominatedAgain.SingleOrDefaultAsync(predicate, cancellationToken);
    }
}