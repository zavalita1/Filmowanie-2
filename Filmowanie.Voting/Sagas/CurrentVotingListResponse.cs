using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Voting.Sagas;

public class CurrentVotingListResponse
{
    public IReadOnlyEmbeddedMovieWithVotes[]? Movies { get; set; }
}