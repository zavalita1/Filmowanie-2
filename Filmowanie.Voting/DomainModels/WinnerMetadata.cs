namespace Filmowanie.Voting.DomainModels;

internal readonly record struct WinnerMetadata(string Id, string Name, string OriginalTitle, int CreationYear, string NominatedBy, DateTime Watched);