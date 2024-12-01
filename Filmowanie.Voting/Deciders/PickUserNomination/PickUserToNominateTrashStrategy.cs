using Filmowanie.Abstractions;
using Filmowanie.Voting.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Deciders.PickUserNomination;

public sealed class PickUserToNominateTrashStrategy : IPickUserToNominateStrategy
{
    private readonly ILogger<PickUserToNominateTrashStrategy> _log;

    public PickUserToNominateTrashStrategy(ILogger<PickUserToNominateTrashStrategy> log)
    {
        _log = log;
    }

    public DomainUser GetUserToNominate(IDictionary<DomainUser, PickUserToNominateContext> userContexts)
    {
        var userScores = new Dictionary<DomainUser, int>(userContexts.Count);
        var userContextToConsider = userContexts
            .Where(x => x.Value.VoteForMovie is { Type: MovieVoteType.Trash })
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

    private (DomainUser User, decimal Score) GetScore(KeyValuePair<DomainUser, PickUserToNominateContext> context)
    {
        var nominationPendingComponent = (decimal)Math.Pow(1.2, Math.Floor(context.Value.NominationPendingTimeInDaysMean));
        var participationComponent = 1 / (1 + 5 * context.Value.VotingParticipationPercent);
        var previousNominationsLength = context.Value.PreviousNominations.Length + context.Value.PendingNominationsCount;
        var result = -1 * previousNominationsLength * nominationPendingComponent * participationComponent;
        var previousNominationsString = GetPreviousNominationsString(context.Value.PreviousNominations);
        _log.LogInformation("{type}: user: {user} participation percent: {percent}, previous nominations: {prev}, giving score: {score}."
            , nameof(PickUserToNominateTrashStrategy),
            context.Key.Name, context.Value.VotingParticipationPercent, previousNominationsString, result);
        return (context.Key, result);
    }

    private static string GetPreviousNominationsString(IEnumerable<IMovie> movies) => movies.Count().ToString();
}