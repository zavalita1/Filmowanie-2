using Filmowanie.Nomination.Handlers;

namespace Filmowanie.Nomination.Interfaces;

internal interface IFilmwebPathResolver
{
    FilmwebUriMetadata GetMetadata(string filmbwebMovieAbsolutePath);
}