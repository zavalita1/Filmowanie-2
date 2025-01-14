using Filmowanie.Abstractions.Extensions;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Extensions;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.Deciders.PickUserNomination;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.Interfaces;

namespace Filmowanie.Voting.Retrievers;

internal sealed class NominationsRetriever : INominationsRetriever
{
    private readonly IPickUserToNominateStrategyFactory _pickUserToNominateStrategyFactory;

    public NominationsRetriever(IPickUserToNominateStrategyFactory pickUserToNominateStrategyFactory)
    {
        _pickUserToNominateStrategyFactory = pickUserToNominateStrategyFactory;
    }

    public List<EmbeddedUserWithNominationAward> GetNominations(Dictionary<IReadOnlyEmbeddedUser, PickUserToNominateContext> assignNominationsUserContexts, VotingConcludedEvent message, VotingResults votingResults)
    {
        var pickUserNominationStrategy = _pickUserToNominateStrategyFactory.ForRegularVoting();
        var userToNominate = pickUserNominationStrategy.GetUserToNominate(votingResults.Winner, assignNominationsUserContexts);

        var list = new List<EmbeddedUserWithNominationAward>();
        var winnerAward = new EmbeddedUserWithNominationAward { AwardMessage = "TODO", Decade = votingResults.Winner.MovieCreationYear.ToDecade(), User = userToNominate.AsMutable() };
        list.Add(winnerAward);

        var trashDecider = _pickUserToNominateStrategyFactory.ForTrashVoting();
        foreach (var trashMovie in votingResults.MoviesGoingByeBye)
        {
            var trashNominator = trashDecider.GetUserToNominate(trashMovie, assignNominationsUserContexts);
            var decade = message.MoviesWithVotes.Single(x => x.Movie.id == trashMovie.id).Movie.MovieCreationYear.ToDecade();
            list.Add(new EmbeddedUserWithNominationAward { AwardMessage = "TODO 22", Decade = decade, User = trashNominator.AsMutable() });
        }

        return list;
    }
}