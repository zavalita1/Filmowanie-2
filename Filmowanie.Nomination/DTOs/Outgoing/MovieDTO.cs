namespace Filmowanie.Nomination.DTOs.Outgoing;

public sealed record MovieDTO(string MovieId, string MovieName, string PosterUrl, string BigPosterUrl, string Description, string FilmwebUrl, int CreatedYear, string Duration, string[] Genres, string[] Actors, string[] Directors, string[] Writers, string OriginalTitle, bool IsPlaceholder)
{
    public MovieDTO(string MovieId, string MovieName, string PosterUrl, string BigPosterUrl, string Description, string FilmwebUrl, int CreatedYear, string Duration, string[] Genres, string[] Actors, string[] Directors, string[] Writers, string OriginalTitle) : this(MovieId, MovieName, PosterUrl, BigPosterUrl, Description, FilmwebUrl, CreatedYear, Duration, Genres, Actors, Directors, Writers, OriginalTitle, false)
    {
    }
}