namespace Filmowanie.Voting.DTOs.Outgoing;

public sealed record VotingSessionStatusDto(string Status, string? CurrentVotingSessionId);