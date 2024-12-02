namespace Filmowanie.Voting.DTOs.Outgoing;

public sealed record VotingResultRowDTO(string MovieName, int VotersCount, bool IsWinner);