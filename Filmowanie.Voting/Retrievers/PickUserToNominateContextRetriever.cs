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
        var groups = lastVotingResults
            .SelectMany(x => x.MoviesAdded)
            .Concat(moviesAdded)
            .GroupBy(x => x.NominatedBy, x => x.NominationConcluded.Subtract(x.NominationStarted), ReadOnlyEmbeddedUserEqualityComparer.Instance)
            .Select(x => (x.Key, x.ToArray()));

        var missingUsers = message.MoviesWithVotes.SelectMany(x => x.Votes).Select(x => x.User).Where(x => groups.All(y => y.Key.id != x.id));
        var userContextsPreCalculationMap = groups
            .Concat(missingUsers.Select(y => (y, Array.Empty<TimeSpan>())))
            .ToDictionary(x => x.Item1, x => x.Item2, ReadOnlyEmbeddedUserEqualityComparer.Instance);

        var userVotesMap = message.MoviesWithVotes
            .SelectMany(x => x.Votes.Select(y => new { MovieId = x.Movie.id, Vote = y }))
            .GroupBy(x => x.Vote.User, x => new { x.MovieId, x.Vote.VoteType }, ReadOnlyEmbeddedUserEqualityComparer.Instance)
            .ToDictionary(x => x.Key, x => x.ToArray(), ReadOnlyEmbeddedUserEqualityComparer.Instance);

        var contextsDictionary = new Dictionary<IReadOnlyEmbeddedUser, PickUserToNominateContext>(userContextsPreCalculationMap.Count());

        foreach (var group in userContextsPreCalculationMap)
        {
            var averageNominationPendingTime = group.Value.Length == 0 ? 0 : group.Value.Average(y => y.TotalDays);
            var votingTookPartIn = 1 + lastVotingResults.Count(x => x.Movies.SelectMany(y => y.Votes).Any(y => y.User.id == group.Key.id));
            var votes = userVotesMap[group.Key].Select(x => (x.MovieId, x.VoteType)).ToArray();

            var entry = new PickUserToNominateContext
            {
                AverageNominationPendingTimeInDays = averageNominationPendingTime,
                NominationsCount = group.Value.Length,
                ParticipationPercent = (100d / (lastVotingResults.Length + 1)) * votingTookPartIn,
                Votes = votes
            };
            contextsDictionary[group.Key] = entry;
        }

        return contextsDictionary;
    }
}