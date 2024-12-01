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

    public UserId GetUserToNominate(IDictionary<UserId, PickUserToNominateContext> userContexts)
    {
        var userScores = new Dictionary<UserId, int>(userContexts.Count);
        var rankingsFromNominationsCounts =
            userContexts
                .Select(SortBy)
                .GetExAeuquoRankings(x => x.Score, x => x.User);

        for (var index = 0; index < userContexts.Count; index++)
        {
            var pickUserToNominateContext = userContexts.ElementAt(index);
            userScores[pickUserToNominateContext.Key] = rankingsFromNominationsCounts[pickUserToNominateContext.Key];

            if (pickUserToNominateContext.Value.VoteForMovie is { Type: MovieVoteType.Trash })
            {
                userScores[pickUserToNominateContext.Key] += userContexts.Count / 2;
                _log.LogInformation("{type} User: {user} get bonus for voting for trash.", 
                    nameof(PickUserToNominateStrategy), pickUserToNominateContext.Key.Name);
            }

        }

        return userScores.MaxBy(x => x.Value).Key;
    }

    private (UserId User, decimal Score) SortBy(KeyValuePair<UserId, PickUserToNominateContext> context)
    {
        var nominationPendingComponent = (decimal)Math.Pow(1.2, Math.Floor(context.Value.NominationPendingTimeInDaysMean));
        var participationComponent = 1 / (1 + 5 * context.Value.VotingParticipationPercent);
        var previousNominationsLength = context.Value.PreviousNominations.Length + context.Value.PendingNominationsCount;
        var result = -1 * previousNominationsLength * nominationPendingComponent * participationComponent;
        var previousNominationsString = GetPreviousNominationsString(context.Value.PreviousNominations);
        _log.LogInformation("{type}: user: {user} participation percent: {percent}, previous nominations: {prev}, avg. nomination pending time: {pending} giving score: {score}."
            , nameof(PickUserToNominateStrategy),
            context.Key.Name, context.Value.VotingParticipationPercent, previousNominationsString, context.Value.NominationPendingTimeInDaysMean, result);
        return (context.Key, result);
    }

    private static string GetPreviousNominationsString(IEnumerable<IMovie> movies) => movies.Count().ToString();
}