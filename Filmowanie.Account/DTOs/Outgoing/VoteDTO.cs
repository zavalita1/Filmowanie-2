namespace Filmowanie.DTOs.Outgoing;

public sealed record VotingResultDTO(VotingResultRowDTO[] VotingResults, TrashVotingResultRowDTO[] TrashVotingResults);