namespace Filmowanie.DTOs.Outgoing;

public sealed record HistoryEntryDTO(string Title, string OriginalTitle, int CreatedYear, string NominatedBy, string Watched);