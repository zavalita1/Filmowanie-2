using Filmowanie.Abstractions;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Nomination.Handlers;

namespace Filmowanie.Nomination.Interfaces;

internal interface IFilmwebHandler
{
    Task<IReadOnlyMovieEntity> GetMovie(FilmwebUriMetadata metadata, TenantId tenant, string? posterUrl, CancellationToken cancel);
}