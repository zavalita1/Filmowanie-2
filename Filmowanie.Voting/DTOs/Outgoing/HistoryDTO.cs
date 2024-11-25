namespace Filmowanie.DTOs.Outgoing;

public sealed record HistoryDTO(HistoryEntryDTO[] Entries);
public sealed record VotingSessionDTO(int Id, string Ended, string EndedUnlocalized);
public sealed record VotingSessionsDTO(VotingSessionDTO[] VotingSessions);