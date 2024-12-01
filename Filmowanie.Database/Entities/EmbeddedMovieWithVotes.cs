using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities;

public class EmbeddedMovieWithVotes : EmbeddedMovie, IReadOnlyEmbeddedMovieWithVotes
{
    public IEnumerable<Vote> Votes { get; set; }

    IEnumerable<IReadOnlyVote> IReadOnlyEmbeddedMovieWithVotes.Votes => Votes;
}

public class ResultEmbeddedMovie : IResultEmbeddedMovie
{
    public IReadOnlyEmbeddedMovieWithVotes Movie { get; set; }
    public int VotingScore { get; set; }
}