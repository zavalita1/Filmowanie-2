using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.Deciders;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.Interfaces;

namespace Filmowanie.Voting.Retrievers;

internal sealed class VotingResultsRetriever : IVotingResultsRetriever
{
    private readonly IVotingDeciderFactory _votingDeciderFactory;

    public VotingResultsRetriever(IVotingDeciderFactory votingDeciderFactory)
    {
        _votingDeciderFactory = votingDeciderFactory;
    }

    public VotingResults GetVotingResults(IReadOnlyEmbeddedMovieWithVotes[] currentMovies, IReadOnlyVotingResult? previous)
    {
        var regularDecider = _votingDeciderFactory.ForRegularVoting();
        var previousMovies = previous?.Movies.ToArray() ?? [];
        var moviesWithScores = regularDecider.AssignScores(currentMovies, previousMovies).ToArray();

        var trashVotingDecider = _votingDeciderFactory.ForTrashVoting();
        var moviesWithTrashScore = trashVotingDecider.AssignScores(currentMovies, previousMovies);

        var moviesGoingByeBye = moviesWithTrashScore.Where(x => x.IsWinner).Select(IReadOnlyEmbeddedMovie (x) => x.Movie.Movie).ToArray();
        var winner = moviesWithScores.Single(x => x.IsWinner).Movie;
        var movies = moviesWithScores.Select(x => x.Movie).OrderByDescending(x => x.VotingScore).ThenByDescending(x => x.Movie == winner.Movie ? 1 : 0).ToArray();

        return new(moviesGoingByeBye, movies, winner.Movie);
    }
}