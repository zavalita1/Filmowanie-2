namespace Filmowanie.Voting.DTOs.Outgoing;

public sealed record TrashVotingResultRowDTO(string MovieName, string[] Voters, bool IsAwarded);