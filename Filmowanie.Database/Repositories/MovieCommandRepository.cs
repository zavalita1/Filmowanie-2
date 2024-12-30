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

    public async Task InsertCanBeNominatedAgainAsync(IEnumerable<IReadOnlyCanNominateMovieAgainEvent> canNominateMovieAgainEvents, CancellationToken cancellationToken)
    {
        var entity = canNominateMovieAgainEvents.Select(x => new CanNominateMovieAgainEvent(x));
        await _ctx.CanNominateMovieAgainEvents.AddRangeAsync(entity, cancellationToken);
        await _ctx.SaveChangesAsync(cancellationToken);
    }

    public async Task InsertNominatedAgainAsync(IReadOnlyNominatedMovieAgainEvent nominatedAgainEvent, CancellationToken cancellationToken)
    {
        var entity = nominatedAgainEvent.AsMutable();
        await _ctx.NominatedMovieAgainEvents.AddAsync(entity, cancellationToken);
        await _ctx.SaveChangesAsync(cancellationToken);
    }

    public Task InsertMovieAsync(IReadOnlyMovieEntity movieEntity, CancellationToken cancellationToken)
    {
        var entity = movieEntity.AsMutable();
        _ctx.Movies.Add(entity);
        return _ctx.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateMovieAsync(string entityId, string posterUrl, CancellationToken cancellationToken)
    {
        var movie = await _ctx.Movies.SingleAsync(x => x.id == entityId, cancellationToken: cancellationToken);
        movie.PosterUrl = posterUrl;
        await _ctx.SaveChangesAsync(cancellationToken);
    }
}