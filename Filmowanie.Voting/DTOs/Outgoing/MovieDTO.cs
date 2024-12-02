namespace Filmowanie.Voting.DTOs.Outgoing;

public sealed record MovieDTO(string MovieId, string MovieName, int Votes, string PosterUrl, string Description, string FilmwebUrl, int CreatedYear, string Duration, string[] Genres, string[] Actors, string[] Directors, string[] Writers, string OriginalTitle, bool IsPlaceholder)
{
    public MovieDTO(string MovieId, string MovieName, int Votes, string PosterUrl, string Description, string FilmwebUrl, int CreatedYear, string Duration, string[] Genres, string[] Actors, string[] Directors, string[] Writers, string OriginalTitle) : this(MovieId, MovieName, Votes, PosterUrl, Description, FilmwebUrl, CreatedYear, Duration, Genres, Actors, Directors, Writers, OriginalTitle, false)
    {
    }

    public MovieDTO(string placeholderTitle, int year) : this("", placeholderTitle, 0, "", "", "", year, "", [], [],[], [], "", true)
    {
    }
}