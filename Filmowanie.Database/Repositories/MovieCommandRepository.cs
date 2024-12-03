using Filmowanie.Database.Contexts;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Extensions;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Microsoft.EntityFrameworkCore;

namespace Filmowanie.Database.Repositories;

internal sealed class MovieCommandRepository : IMovieCommandRepository
{
    private readonly MoviesContext _ctx;

    public MovieCommandRepository(MoviesContext ctx)
    {
        _ctx = ctx;
    }

    public async Task UpdateMoviesThatCanBeNominatedAgainEntityAsync(string entityId, IEnumerable<IReadOnlyEmbeddedMovie> movies, CancellationToken cancellationToken)
    {
        var entity = await _ctx.MoviesThatCanBeNominatedAgain.SingleAsync(x => x.Id == entityId, cancellationToken);
        entity.Movies = movies.Select(IReadOnlyEntitiesExtensions.AsMutable).ToArray();
        await _ctx.SaveChangesAsync(cancellationToken);
    }

    public Task InsertMovieAsync(IReadOnlyMovieEntity movieEntity, CancellationToken cancellationToken)
    {
        var entity = new MovieEntity(movieEntity);
        _ctx.Movies.Add(entity);
        return _ctx.SaveChangesAsync(cancellationToken);
    }
}