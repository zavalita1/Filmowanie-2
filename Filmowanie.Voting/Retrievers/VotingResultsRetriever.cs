using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.Interfaces;

namespace Filmowanie.Voting.Retrievers;

// TODO UTs
internal sealed class VotingResultsRetriever : IVotingResultsRetriever
{
    private readonly IVotingDeciderFactory votingDeciderFactory;

    public VotingResultsRetriever(IVotingDeciderFactory votingDeciderFactory)
    {
        this.votingDeciderFactory = votingDeciderFactory;
    }

    public VotingResults GetVotingResults(IReadOnlyEmbeddedMovieWithVotes[] currentMovies, IReadOnlyVotingResult? previous)
    {
        var regularDecider = this.votingDeciderFactory.ForRegularVoting();
        var previousMovies = previous?.Movies.ToArray() ?? [];
        var moviesWithScores = regularDecider.AssignScores(currentMovies, previousMovies).ToArray();

        var trashVotingDecider = this.votingDeciderFactory.ForTrashVoting();
        var moviesWithTrashScore = trashVotingDecider.AssignScores(currentMovies, previousMovies);

        var moviesGoingByeBye = moviesWithTrashScore.Where(x => x.IsWinner).Select(IReadOnlyEmbeddedMovie (x) => x.Movie.Movie).ToArray();
        var winner = moviesWithScores.Single(x => x.IsWinner).Movie;
        var movies = moviesWithScores.Select(x => x.Movie).OrderByDescending(x => x.VotingScore).ThenByDescending(x => x.Movie == winner.Movie ? 1 : 0).ToArray();

        return new(moviesGoingByeBye, movies, winner.Movie);
    }
}