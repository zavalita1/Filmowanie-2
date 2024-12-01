using Filmowanie.Abstractions.Enums;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.Extensions;
using Filmowanie.Voting.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Deciders.PickUserNomination;

public sealed class PickUserToNominateStrategy : IPickUserToNominateStrategy
{
    private readonly ILogger<PickUserToNominateStrategy> _log;

    public PickUserToNominateStrategy(ILogger<PickUserToNominateStrategy> log)
    {
        _log = log;
    }

    public IReadOnlyEmbeddedUser GetUserToNominate(IReadOnlyEmbeddedMovie movieToReplace, IDictionary<IReadOnlyEmbeddedUser, PickUserToNominateContext> userContexts)
    {
        var userScores = new Dictionary<IReadOnlyEmbeddedUser, int>(userContexts.Count);
        var rankingsFromNominationsCounts =
            userContexts
                .Select(SortBy)
                .GetExAeuquoRankings(x => x.Score, x => x.User);

        for (var index = 0; index < userContexts.Count; index++)
        {
            var pickUserToNominateContext = userContexts.ElementAt(index);
            userScores[pickUserToNominateContext.Key] = rankingsFromNominationsCounts[pickUserToNominateContext.Key];

            if (pickUserToNominateContext.Value.Votes.Any(x => x.Item2 == VoteType.Thrash && x.MovieId == movieToReplace.id))
            {
                userScores[pickUserToNominateContext.Key] += userContexts.Count / 2;
                _log.LogInformation("{type} User: {user} get bonus for voting for trash.",
                    nameof(PickUserToNominateStrategy), pickUserToNominateContext.Key.Name);
            }

        }

        return userScores.MaxBy(x => x.Value).Key;
    }

    private (IReadOnlyEmbeddedUser User, double Score) SortBy(KeyValuePair<IReadOnlyEmbeddedUser, PickUserToNominateContext> context)
    {
        var nominationPendingComponent = Math.Pow(1.2, Math.Floor(context.Value.AverageNominationPendingTimeInDays));
        var participationComponent = 1 / (1 + 5 * context.Value.ParticipationPercent);
        var previousNominationsLength = context.Value.NominationsCount;
        var result = -1 * previousNominationsLength * nominationPendingComponent * participationComponent;

        _log.LogInformation("{type}: user: {user} participation percent: {percent}, previous nominations: {prev}, avg. nomination pending time: {pending} giving score: {score}."
            , nameof(PickUserToNominateStrategy),
            context.Key.Name, context.Value.ParticipationPercent, context.Value.NominationsCount, context.Value.AverageNominationPendingTimeInDays, result);
        return (context.Key, result);
    }
}