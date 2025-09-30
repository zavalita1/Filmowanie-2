namespace Filmowanie.Voting.DTOs.Outgoing;

public sealed record MovieDTO(string MovieId, string MovieName, int Votes, string PosterUrl, string BigPosterUrl, string Description, string AltDescription, string FilmwebUrl, int CreatedYear, string Duration, string[] Genres, string[] Actors, string[] Directors, string[] Writers, string OriginalTitle, bool IsPlaceholder)
{
    public MovieDTO(
        string MovieId, 
        string MovieName, 
        int Votes, 
        string PosterUrl, 
        string BigPosterUrl, 
        string Description,
        string AltDescription,
        string FilmwebUrl, 
        int CreatedYear, 
        string Duration, 
        string[] Genres, 
        string[] Actors, 
        string[] Directors, 
        string[] Writers, 
        string OriginalTitle) : this(MovieId, MovieName, Votes, PosterUrl, BigPosterUrl, Description, AltDescription, FilmwebUrl, CreatedYear, Duration, Genres, Actors, Directors, Writers, OriginalTitle, false)
    {
    }

    public MovieDTO(string placeholderTitle, int year) : this("", placeholderTitle, 0, "", "", "", "", "", year, "", [], [],[], [], "", true)
    {
    }
}

public sealed record HistoryDTO(HistoryEntryDTO[] Entries);

public sealed record HistoryEntryDTO(string Title, string OriginalTitle, int CreatedYear, string NominatedBy, string Watched, string FilmwebUrl);