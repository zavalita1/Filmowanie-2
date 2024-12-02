namespace Filmowanie.Nomination.DTOs.Outgoing;

public sealed record MovieDTO(string MovieId, string MovieName, string PosterUrl, string Description, string FilmwebUrl, int CreatedYear, string Duration, string[] Genres, string[] Actors, string[] Directors, string[] Writers, string OriginalTitle, bool IsPlaceholder)
{
    public MovieDTO(string MovieId, string MovieName, string PosterUrl, string Description, string FilmwebUrl, int CreatedYear, string Duration, string[] Genres, string[] Actors, string[] Directors, string[] Writers, string OriginalTitle) : this(MovieId, MovieName, PosterUrl, Description, FilmwebUrl, CreatedYear, Duration, Genres, Actors, Directors, Writers, OriginalTitle, false)
    {
    }
}