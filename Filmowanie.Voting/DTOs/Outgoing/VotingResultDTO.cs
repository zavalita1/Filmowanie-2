namespace Filmowanie.Voting.DTOs.Outgoing;

public sealed record VotingResultDTO(VotingResultRowDTO[] VotingResults, TrashVotingResultRowDTO[] TrashVotingResults);

public sealed record VotingSessionDTO(string Id, string Ended, string EndedUnlocalized);
public sealed record VotingSessionsDTO(VotingSessionDTO[] VotingSessions);