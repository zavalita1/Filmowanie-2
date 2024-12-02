namespace Filmowanie.Voting.DTOs.Outgoing;

public sealed record MovieVotingStandingsDTO(string MovieTitle, int?[] VotingPlaces, int?[] VotesReceived);