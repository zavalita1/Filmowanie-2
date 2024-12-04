using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Extensions;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.Extensions;
using Filmowanie.Voting.Interfaces;

namespace Filmowanie.Voting.Deciders;

public sealed class VotingDecider : IVotingDecider
{
    public IEnumerable<(IReadOnlyEmbeddedMovieWithVotes Movie, bool IsWinner)> AssignScores(IEnumerable<IReadOnlyEmbeddedMovieWithVotes> moviesWithVotes,
        IEnumerable<IReadOnlyEmbeddedMovieWithVotes> previousVotingMoviesWithVotes)
    {
        var previousVotingByMovies = previousVotingMoviesWithVotes.ToDictionary(x => x.Movie.id, x => GetVotesCount(x.Votes));

        var sorted = moviesWithVotes
            .Select(x => new { MovieContainer = x, CurrentVotes = GetVotesCount(x.Votes), PreviousVotes = previousVotingByMovies.GetValueOrDefault(x.Movie.id, 0) })
            .OrderByDescending(x => x.CurrentVotes)
            .ThenBy(x => x.PreviousVotes)
            .ToArray();

        var result = new EmbeddedMovieWithVotes { Movie = sorted[0].MovieContainer.Movie.AsMutable(), VotingScore = sorted[0].CurrentVotes };
        yield return (result, true);

        foreach (var loser in sorted.Skip(1))
        {
            var loserResult = new EmbeddedMovieWithVotes { Movie = loser.MovieContainer.Movie.AsMutable(), VotingScore = loser.CurrentVotes };
            yield return (loserResult, false);
        }
    }

    private static int GetVotesCount(IEnumerable<IReadOnlyVote> votes) => votes.Select(x => x.VoteType).Sum(VoteTypeExtensions.GetVoteCount);
}