namespace Filmowanie.DTOs.Outgoing;

public sealed record VotingResultRowDTO(string MovieName, int VotersCount, bool IsWinner);