using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Extensions;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities;

public class EmbeddedMovieWithVotes : IReadOnlyEmbeddedMovieWithVotes
{
    public IEnumerable<Vote> Votes { get; set; } = new List<Vote>();

    public EmbeddedMovie Movie { get; set; } = null!;

    public int VotingScore { get; set; }

    IEnumerable<IReadOnlyVote> IReadOnlyEmbeddedMovieWithVotes.Votes => Votes;
    IReadOnlyEmbeddedMovie IReadOnlyEmbeddedMovieWithVotes.Movie => Movie;

    public EmbeddedMovieWithVotes(IReadOnlyEmbeddedMovieWithVotes other) : this()
    {
        Votes = other.Votes.Select(x => x.AsMutable());
        Movie = other.Movie.AsMutable();
        VotingScore = other.VotingScore;
    }

    public EmbeddedMovieWithVotes(IReadOnlyEmbeddedMovie other) : this()
    {
        Movie = other.AsMutable();
    }

    public EmbeddedMovieWithVotes()
    {
        Votes = new List<Vote>();
        VotingScore = 0;
    }
}
