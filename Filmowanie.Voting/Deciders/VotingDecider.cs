using Filmowanie.Abstractions.Enums;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.Interfaces;

namespace Filmowanie.Voting.Deciders;

public sealed class VotingDecider : IVotingDecider
{
    public IEnumerable<(IResultEmbeddedMovie Movie, bool IsWinner)> AssignScores(IEnumerable<IResultEmbeddedMovie> moviesWithVotes, IEnumerable<IReadOnlyEmbeddedMovieWithVotes> previousVotingMoviesWithVotes)
    {
        var previousVotingByMovies = previousVotingMoviesWithVotes.ToDictionary(x => x.id, x => GetVotesCount(x.Votes));

        var sorted = moviesWithVotes
            .Select(x => x.Movie)
            .Select(x => new { Movie = x, CurrentVotes = GetVotesCount(x.Votes), PreviousVotes = previousVotingByMovies.GetValueOrDefault(x.id, 0) })
            .OrderByDescending(x => x.CurrentVotes)
            .ThenBy(x => x.PreviousVotes)
            .ToArray();

        var result = new ResultEmbeddedMovie { Movie = sorted[0].Movie, VotingScore = sorted[0].CurrentVotes };
        yield return (result, true);

        foreach (var loser in sorted.Skip(1))
        {
            var loserResult = new ResultEmbeddedMovie { Movie = loser.Movie, VotingScore = loser.CurrentVotes };
            yield return (loserResult, false);
        }
    }

    private static int GetVotesCount(IEnumerable<IReadOnlyVote> votes) => votes.Sum(x => x.VoteType == VoteType.Thrash ? 0 : (int)x.VoteType);
}