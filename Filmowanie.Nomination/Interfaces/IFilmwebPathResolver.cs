using Filmowanie.Nomination.Services;

namespace Filmowanie.Nomination.Interfaces;

internal interface IFilmwebPathResolver
{
    FilmwebUriMetadata GetMetadata(string filmbwebMovieAbsolutePath);
}