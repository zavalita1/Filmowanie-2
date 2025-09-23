using Filmowanie.Database.Contexts;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using System.Linq.Expressions;
using Filmowanie.Database.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Filmowanie.Database.Repositories;

internal sealed class MovieQueryRepository : IMovieQueryRepository
{
    private readonly MoviesContext ctx;

    public MovieQueryRepository(MoviesContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task<IReadOnlyMovieEntity[]> GetMoviesAsync(Expression<Func<IReadOnlyMovieEntity, bool>> predicate, CancellationToken cancelToken)
    {
        return await this.ctx.Movies.Where(predicate).ToArrayAsync(cancelToken);
    }

    public async Task<IReadOnlyCanNominateMovieAgainEvent[]> GetMoviesThatCanBeNominatedAgainEventsAsync(Expression<Func<IReadOnlyCanNominateMovieAgainEvent, bool>> predicate,
        CancellationToken cancelToken)
    {
        return await this.ctx.CanNominateMovieAgainEvents.Where(predicate).ToArrayAsync(cancelToken);
    }

    public async Task<IReadOnlyNominatedMovieEvent[]> GetMovieNominatedEventsAsync(Expression<Func<IReadOnlyNominatedMovieEvent, bool>> predicate,
        CancellationToken cancelToken)
    {
        var res = await this.ctx.NominatedMovieEvents.Where(predicate).ToArrayAsync(cancelToken);
        return res;
    }
}