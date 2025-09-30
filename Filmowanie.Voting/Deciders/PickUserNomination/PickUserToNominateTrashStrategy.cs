using Filmowanie.Abstractions.Enums;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.Extensions;
using Filmowanie.Voting.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Deciders.PickUserNomination;

// TODO UTs
public sealed class PickUserToNominateTrashStrategy : IPickUserToNominateStrategy
{
    private readonly ILogger<PickUserToNominateTrashStrategy> log;

    public PickUserToNominateTrashStrategy(ILogger<PickUserToNominateTrashStrategy> log)
    {
        this.log = log;
    }

    public IReadOnlyEmbeddedUser GetUserToNominate(IReadOnlyEmbeddedMovie movieToReplace, IDictionary<IReadOnlyEmbeddedUser, PickUserToNominateContext> userContexts)
    {
        var userScores = new Dictionary<IReadOnlyEmbeddedUser, int>(userContexts.Count);
        var userContextToConsider = userContexts
            .Where(x => x.Value.Votes.Any(y => y.Item2 == VoteType.Trash && y.MovieId == movieToReplace.id))
            .Select(GetScore)
            .ToArray();

        // try excluding 0-scored folks (i.e. newcomers) unless it's all newcomers that voted.
        userContextToConsider = userContextToConsider.Any(x => x.Score != 0) 
            ? userContextToConsider.Where(x => x.Score != 0).ToArray() 
            : userContextToConsider;

        var nominationsRanks =
            userContextToConsider.GetExAeuquoRankings(x => x.Score, x => x.User);
        
        foreach (var t in userContextToConsider)
        {
            userScores[t.User] = nominationsRanks[t.User];
        }

        return userScores.MaxBy(x => x.Value).Key;
    }

    private (IReadOnlyEmbeddedUser User, double Score) GetScore(KeyValuePair<IReadOnlyEmbeddedUser, PickUserToNominateContext> context)
    {
        var nominationPendingComponent = Math.Pow(1.2, Math.Floor(context.Value.AverageNominationPendingTimeInDays));
        var participationComponent = 1 / (1 + 5 * context.Value.ParticipationFactor);
        var previousNominationsLength = context.Value.NominationsCount;
        var result = -1 * previousNominationsLength * nominationPendingComponent * participationComponent;

        this.log.LogInformation("{type}: user: {user} participation percent: {percent}, previous nominations: {prev}, giving score: {score}."
            , nameof(PickUserToNominateTrashStrategy),
            context.Key.Name, context.Value.ParticipationFactor, context.Value.NominationsCount, result);
        return (context.Key, result);
    }
}