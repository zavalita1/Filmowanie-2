namespace Filmowanie.Voting.DTOs.Outgoing;

public sealed record MovieDTO(string MovieName, int Votes, string PosterUrl, string Description, string FilmwebUrl, int CreatedYear, string Duration, string[] Genres, string[] Actors, string[] Directors, string[] Writers, string OriginalTitle, bool IsPlaceholder)
{
    public MovieDTO(string MovieName, int Votes, string PosterUrl, string Description, string FilmwebUrl, int CreatedYear, string Duration, string[] Genres, string[] Actors, string[] Directors, string[] Writers, string OriginalTitle) : this(MovieName, Votes, PosterUrl, Description, FilmwebUrl, CreatedYear, Duration, Genres, Actors, Directors, Writers, OriginalTitle, false)
    {
    }

    public MovieDTO(string MovieName, int Year) : this(MovieName, 0, "", "", "", Year, "", Array.Empty<string>(), Array.Empty<string>(),Array.Empty<string>(), Array.Empty<string>(), "", true)
    {
    }
}