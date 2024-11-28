namespace Filmowanie.DTOs.Outgoing;

public sealed record MovieDTO(string MovieName, int Votes, string PosterUrl, string Description, string FilmwebUrl, int CreatedYear, string Duration, string[] Genres, string[] Actors, string[] Directors, string[] Writers, string OriginalTitle, bool IsPlaceholder)
{
    public MovieDTO(string MovieName, int Votes, string PosterUrl, string Description, string FilmwebUrl, int CreatedYear, string Duration, string[] Genres, string[] Actors, string[] Directors, string[] Writers, string OriginalTitle) : this(MovieName, Votes, PosterUrl, Description, FilmwebUrl, CreatedYear, Duration, Genres, Actors, Directors, Writers, OriginalTitle, false)
    {
    }

    /// <summary>
    /// Placeholder constructor
    /// </summary>
    /// <param name="Year">First year of a decade to nominate from.</param>
    public MovieDTO(int Year) : this("", 0, "", "", "", Year, "", [], [],[], [], "", true)
    {
    }
}