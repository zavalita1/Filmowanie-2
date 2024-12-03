using Filmowanie.Nomination.Handlers;

namespace Filmowanie.Nomination.Interfaces;

internal interface IFilmwebPostersUrlsRetriever
{
    Task<IEnumerable<string>> GetPosterUrlsAsync(FilmwebUriMetadata metadata, CancellationToken cancel);
}