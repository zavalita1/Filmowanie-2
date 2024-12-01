using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities;

public class EmbeddedMovieWithVotes : IReadOnlyEmbeddedMovieWithVotes
{
    public IEnumerable<Vote> Votes { get; set; } = [];

    public EmbeddedMovie Movie { get; set; }

    public int VotingScore { get; set; }

    IEnumerable<IReadOnlyVote> IReadOnlyEmbeddedMovieWithVotes.Votes => Votes;
    IReadOnlyEmbeddedMovie IReadOnlyEmbeddedMovieWithVotes.Movie => Movie;
}
