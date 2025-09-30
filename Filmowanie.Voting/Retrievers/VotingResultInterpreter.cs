using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Retrievers;

public sealed class VotingResultInterpreter : IVotingResultInterpreter
{
    private readonly ILogger<VotingResultInterpreter> log;

    public VotingResultInterpreter(ILogger<VotingResultInterpreter> log)
    {
        this.log = log;
    }

    public bool IsExtraVotingNecessary(IReadOnlyEmbeddedMovieWithVotes[] moviesWithVotes, out IReadOnlyEmbeddedMovie[] qualifiedMovies)
    {
        var winningScore = moviesWithVotes.Max(x => x.VotingScore);
        var scoreWinners = moviesWithVotes.Where(x => x.VotingScore == winningScore);

        if (scoreWinners.Count() == 1)
        {
            qualifiedMovies = [];
            return false;
        }

        qualifiedMovies = scoreWinners.Select(x => x.Movie).ToArray();
        return true;
    }
}