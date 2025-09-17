using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.Helpers;
using Filmowanie.Voting.Interfaces;

namespace Filmowanie.Voting.Retrievers;

// TODO UTs
internal sealed class PickUserToNominateContextRetriever : IPickUserToNominateContextRetriever
{
    public Dictionary<IReadOnlyEmbeddedUser, PickUserToNominateContext> GetPickUserToNominateContexts(IReadOnlyVotingResult[] lastVotingResults, IReadOnlyEmbeddedMovieWithNominationContext[] moviesAdded, VotingConcludedEvent message)
    {
        var usersWithTheirPastNominationPendingTimes = lastVotingResults
            .SelectMany(x => x.MoviesAdded)
            .Concat(moviesAdded)
            .GroupBy(x => x.NominatedBy, x => x.NominationConcluded.Subtract(x.NominationStarted), ReadOnlyEmbeddedUserEqualityComparer.Instance)
            .Select(x => (x.Key, x.ToArray()));

        var usersWithoutPastNominations = message.MoviesWithVotes.SelectMany(x => x.Votes).Select(x => x.User).Where(x => usersWithTheirPastNominationPendingTimes.All(y => y.Key.id != x.id)).DistinctBy(x => x.id);
        var usersWithTheirPastNominationPendingTimesMap = usersWithTheirPastNominationPendingTimes
            .Concat(usersWithoutPastNominations.Select(y => (y, Array.Empty<TimeSpan>())))
            .ToDictionary(x => x.Item1, x => x.Item2, ReadOnlyEmbeddedUserEqualityComparer.Instance);

        var currentVotingUserVotesMap = message.MoviesWithVotes
            .SelectMany(x => x.Votes.Select(y => new { MovieId = x.Movie.id, Vote = y }))
            .GroupBy(x => x.Vote.User, x => new { x.MovieId, x.Vote.VoteType }, ReadOnlyEmbeddedUserEqualityComparer.Instance)
            .ToDictionary(x => x.Key, x => x.ToArray(), ReadOnlyEmbeddedUserEqualityComparer.Instance);

        var result = new Dictionary<IReadOnlyEmbeddedUser, PickUserToNominateContext>(usersWithTheirPastNominationPendingTimesMap.Count());

        foreach (var userWithPastNominations in usersWithTheirPastNominationPendingTimesMap)
        {
            var averageNominationPendingTime = userWithPastNominations.Value.Length == 0 ? 0 : userWithPastNominations.Value.Average(y => y.TotalDays);
            var votingSessionsUserTookPartInCount = 1 + lastVotingResults.Count(x => x.Movies.SelectMany(y => y.Votes).Any(y => y.User.id == userWithPastNominations.Key.id));

            if (!currentVotingUserVotesMap.TryGetValue(userWithPastNominations.Key, out var votes))
                votes = [];

            var mappedVotes = votes.Select(x => (x.MovieId, x.VoteType)).ToArray();

            var entry = new PickUserToNominateContext
            {
                AverageNominationPendingTimeInDays = averageNominationPendingTime,
                NominationsCount = userWithPastNominations.Value.Length,
                ParticipationFactor = (100d / (lastVotingResults.Length + 1)) * votingSessionsUserTookPartInCount,
                Votes = mappedVotes
            };
            result[userWithPastNominations.Key] = entry;
        }

        return result;
    }
}