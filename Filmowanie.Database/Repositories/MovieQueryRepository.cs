using Filmowanie.Database.Contexts;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using System.Linq.Expressions;
using Filmowanie.Database.Interfaces;
using Microsoft.EntityFrameworkCore;
using Filmowanie.Database.Entities;

namespace Filmowanie.Database.Repositories;

internal sealed class PushSubscriptionRepository
{
    private readonly PushSubscriptionsContext _ctx;

    public PushSubscriptionRepository(PushSubscriptionsContext ctx)
    {
        _ctx = ctx;
    }


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

    public async Task<IReadOnlyCanNominateMovieAgainEvent[]> GetMoviesThatCanBeNominatedAgainEntityAsync(Expression<Func<IReadOnlyCanNominateMovieAgainEvent, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _ctx.CanNominateMovieAgainEvents.Where(predicate).ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyNominatedMovieAgainEvent[]> GetMoviesNominatedAgainEntityAsync(Expression<Func<IReadOnlyNominatedMovieAgainEvent, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _ctx.NominatedMovieAgainEvents.Where(predicate).ToArrayAsync(cancellationToken);
    }
}