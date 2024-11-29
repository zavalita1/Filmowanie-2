namespace Filmowanie.Voting.DTOs.Incoming;

public sealed record VoteDTO(string MovieId, string MovieTitle, int Votes);
