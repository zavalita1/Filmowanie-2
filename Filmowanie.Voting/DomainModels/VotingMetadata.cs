namespace Filmowanie.Voting.DomainModels;

internal readonly record struct VotingMetadata(string VotingSessionId, DateTime Concluded, string WinnerName);
