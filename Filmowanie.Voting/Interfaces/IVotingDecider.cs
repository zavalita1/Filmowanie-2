using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Voting.Interfaces;

public interface IVotingDecider
{
    IEnumerable<(IResultEmbeddedMovie Movie, bool IsWinner)> AssignScores(IEnumerable<IResultEmbeddedMovie> moviesWithVotes, IEnumerable<IReadOnlyEmbeddedMovieWithVotes> previousVotingMoviesWithVotes);
}