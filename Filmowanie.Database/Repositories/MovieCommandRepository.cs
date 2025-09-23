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
    private readonly MoviesContext ctx;

    public MovieCommandRepository(MoviesContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task InsertCanBeNominatedAgainAsync(IEnumerable<IReadOnlyCanNominateMovieAgainEvent> canNominateMovieAgainEvents, CancellationToken cancelToken)
    {
        var entity = canNominateMovieAgainEvents.Select(x => new CanNominateMovieAgainEvent(x));
        await this.ctx.CanNominateMovieAgainEvents.AddRangeAsync(entity, cancelToken);
        await this.ctx.SaveChangesAsync(cancelToken);
    }

    public async Task InsertNominatedAsync(IReadOnlyNominatedMovieEvent nominatedEvent, CancellationToken cancelToken)
    {
        var entity = nominatedEvent.AsMutable();
        await this.ctx.NominatedMovieEvents.AddAsync(entity, cancelToken);
        await this.ctx.SaveChangesAsync(cancelToken);
    }

    public Task InsertMovieAsync(IReadOnlyMovieEntity movieEntity, CancellationToken cancelToken)
    {
        var entity = movieEntity.AsMutable();
        this.ctx.Movies.Add(entity);
        return this.ctx.SaveChangesAsync(cancelToken);
    }

    public async Task UpdateMovieAsync(string entityId, string posterUrl, CancellationToken cancelToken)
    {
        var movie = await this.ctx.Movies.SingleAsync(x => x.id == entityId, cancelToken);
        movie.PosterUrl = posterUrl;
        await this.ctx.SaveChangesAsync(cancelToken);
    }

    public async Task<IReadOnlyMovieEntity> MarkMovieAsRejectedAsync(string entityId, CancellationToken cancelToken)
    {
        var movie = await this.ctx.Movies.AsNoTracking().SingleAsync(x => x.id == entityId, cancelToken);
        movie.IsRejected = true;
        await this.ctx.SaveChangesAsync(cancelToken);
        return movie;
    }
}