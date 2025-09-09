using Filmowanie.Database.Contexts;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Extensions;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Microsoft.EntityFrameworkCore;

namespace Filmowanie.Database.Repositories;

// TODO db tests
internal sealed class MovieCommandRepository : IMovieCommandRepository
{
    private readonly MoviesContext _ctx;

    public MovieCommandRepository(MoviesContext ctx)
    {
        _ctx = ctx;
    }

    public async Task InsertCanBeNominatedAgainAsync(IEnumerable<IReadOnlyCanNominateMovieAgainEvent> canNominateMovieAgainEvents, CancellationToken cancelToken)
    {
        var entity = canNominateMovieAgainEvents.Select(x => new CanNominateMovieAgainEvent(x));
        await _ctx.CanNominateMovieAgainEvents.AddRangeAsync(entity, cancelToken);
        await _ctx.SaveChangesAsync(cancelToken);
    }

    public async Task InsertNominatedAsync(IReadOnlyNominatedMovieEvent nominatedEvent, CancellationToken cancelToken)
    {
        var entity = nominatedEvent.AsMutable();
        await _ctx.NominatedMovieAgainEvents.AddAsync(entity, cancelToken);
        await _ctx.SaveChangesAsync(cancelToken);
    }

    public Task InsertMovieAsync(IReadOnlyMovieEntity movieEntity, CancellationToken cancelToken)
    {
        var entity = movieEntity.AsMutable();
        _ctx.Movies.Add(entity);
        return _ctx.SaveChangesAsync(cancelToken);
    }

    public async Task UpdateMovieAsync(string entityId, string posterUrl, CancellationToken cancelToken)
    {
        var movie = await _ctx.Movies.SingleAsync(x => x.id == entityId, cancelToken);
        movie.PosterUrl = posterUrl;
        await _ctx.SaveChangesAsync(cancelToken);
    }
}