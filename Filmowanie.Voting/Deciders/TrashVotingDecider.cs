using Filmowanie.Abstractions.Enums;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Extensions;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.Interfaces;

namespace Filmowanie.Voting.Deciders;

public sealed class TrashVotingDecider : IVotingDecider
{
    private const int InitialTrashVotesThreshold = 1;

    public IEnumerable<(IReadOnlyEmbeddedMovieWithVotes Movie, bool IsWinner)> AssignScores(IEnumerable<IReadOnlyEmbeddedMovieWithVotes> moviesWithVotes,
        IEnumerable<IReadOnlyEmbeddedMovieWithVotes> previousVotingMoviesWithVotes)
    {
        var threshold = InitialTrashVotesThreshold;

        var previousVotingByMovies = previousVotingMoviesWithVotes.ToDictionary(x => x.Movie.id, x => GetTrashVotesCount(x.Votes));

        var moviesWithVotesCount = moviesWithVotes
            .Select(x => new { MovieContainer = x, CurrentVotes = GetTrashVotesCount(x.Votes), PreviousVotes = previousVotingByMovies.GetValueOrDefault(x.Movie.id, 0) })
            .OrderByDescending(x => x.CurrentVotes)
            .ThenByDescending(x => x.PreviousVotes);

        var previousPair = (-1, -1);
        foreach (var movie in moviesWithVotesCount)
        {
            if ((movie.CurrentVotes, movie.PreviousVotes) != previousPair && movie.CurrentVotes < threshold++)
                yield break;

            previousPair = (movie.CurrentVotes, movie.PreviousVotes);
            var result = new EmbeddedMovieWithVotes { Movie = movie.MovieContainer.Movie.AsMutable(), VotingScore = movie.CurrentVotes};
            yield return (result, true);
        }
    }

    private static int GetTrashVotesCount(IEnumerable<IReadOnlyVote> votes) => votes.Count(x => x.VoteType == VoteType.Thrash);
}
