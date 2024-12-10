namespace Filmowanie.Voting.DTOs.Outgoing;

public sealed record MovieVotingStandingsDTO(string MovieTitle, int?[] VotingPlaces, int?[] VotesReceived);
public sealed record MovieVotingStandingsListDTO(MovieVotingStandingsDTO[] Rows);

