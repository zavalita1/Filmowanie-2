using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.Extensions;
using Filmowanie.Voting.Interfaces;

namespace Filmowanie.Voting.Deciders;

// TODO UTs
public sealed class VotingDecider : IVotingDecider
{
    public IEnumerable<(IReadOnlyEmbeddedMovieWithVotes Movie, bool IsWinner)> AssignScores(IEnumerable<IReadOnlyEmbeddedMovieWithVotes> moviesWithVotes,
        IEnumerable<IReadOnlyEmbeddedMovieWithVotes> previousVotingMoviesWithVotes)
    {
        var previousVotingByMovies = previousVotingMoviesWithVotes.ToDictionary(x => x.Movie.id, x => GetVotesCount(x.Votes));

        var sorted = moviesWithVotes
            .Select(x => new { MovieContainer = x, CurrentVotes = x.Votes, CurrentVotesScore = GetVotesCount(x.Votes), PreviousVotes = previousVotingByMovies.GetValueOrDefault(x.Movie.id, 0) })
            .OrderByDescending(x => x.CurrentVotesScore)
            .ThenBy(x => x.PreviousVotes)
            .ThenBy(x => x.MovieContainer.Movie.MovieCreationYear)
            .ToArray();

        var winner = sorted[0];
        var result = new ReadOnlyEmbeddedMovieWithVotes(winner.MovieContainer.Movie, winner.CurrentVotes, winner.CurrentVotesScore);
        yield return (result, true);

        foreach (var loser in sorted.Skip(1))
        {
            var loserResult = new ReadOnlyEmbeddedMovieWithVotes(loser.MovieContainer.Movie, loser.CurrentVotes, loser.CurrentVotesScore);
            yield return (loserResult, false);
        }
    }

    private static int GetVotesCount(IEnumerable<IReadOnlyVote> votes) => votes.Select(x => x.VoteType).Sum(VoteTypeExtensions.GetVoteCount);

    private readonly record struct ReadOnlyEmbeddedMovieWithVotes(IReadOnlyEmbeddedMovie Movie, IEnumerable<IReadOnlyVote> Votes, int VotingScore) : IReadOnlyEmbeddedMovieWithVotes;
}