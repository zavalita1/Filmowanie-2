using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Voting.DomainModels;

public class CurrentVotingListResponse
{
    public IReadOnlyEmbeddedMovieWithVotes[]? Movies { get; set; }
}