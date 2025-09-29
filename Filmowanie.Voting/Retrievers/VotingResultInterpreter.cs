using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.DomainModels;
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

    public bool IsExtraVotingNecessary(VotingResults results, out IReadOnlyEmbeddedMovie[] qualifiedMovies)
    {
        var winningScore = results.Movies.Max(x => x.VotingScore);

        var scoreWinners = results.Movies.Where(x => x.VotingScore == winningScore);
        if (scoreWinners.All(x => x.Movie.id != results.Winner.id))
        {
            this.log.LogWarning("Cannot determine if there should be extra voting or not, as winner was chosen despite having lower score. If this wasn't by design this particual time - troubleshoot!");
            qualifiedMovies = [];
            return false;
        }

        if (scoreWinners.Count() == 1)
        {
            qualifiedMovies = [];
            return false;
        }

        qualifiedMovies = scoreWinners.Select(x => x.Movie).ToArray();
        return true;
    }
}