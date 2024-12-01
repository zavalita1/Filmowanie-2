using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Voting.Interfaces;

public interface IVotingDecider
{
    IEnumerable<(IReadOnlyEmbeddedMovieWithVotes Movie, bool IsWinner)> AssignScores(IEnumerable<IReadOnlyEmbeddedMovieWithVotes> moviesWithVotes,
        IEnumerable<IReadOnlyEmbeddedMovieWithVotes> previousVotingMoviesWithVotes);
}