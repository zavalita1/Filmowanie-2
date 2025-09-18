using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.Interfaces;

namespace Filmowanie.Voting.Retrievers;

// TODO UTs
internal sealed class NominationsRetriever : INominationsRetriever
{
    private readonly IPickUserToNominateStrategyFactory _pickUserToNominateStrategyFactory;

    public NominationsRetriever(IPickUserToNominateStrategyFactory pickUserToNominateStrategyFactory)
    {
        _pickUserToNominateStrategyFactory = pickUserToNominateStrategyFactory;
    }

    public List<IReadOnlyEmbeddedUserWithNominationAward> GetNominations(Dictionary<IReadOnlyEmbeddedUser, PickUserToNominateContext> assignNominationsUserContexts, VotingConcludedEvent message, VotingResults votingResults)
    {
        var pickUserNominationStrategy = _pickUserToNominateStrategyFactory.ForRegularVoting();
        var userToNominate = pickUserNominationStrategy.GetUserToNominate(votingResults.Winner, assignNominationsUserContexts);

        var list = new List<IReadOnlyEmbeddedUserWithNominationAward>();
        var winnerAward = new ReadOnlyEmbeddedUserWithNominationAward(userToNominate, "TODO", votingResults.Winner.MovieCreationYear.ToDecade());
        list.Add(winnerAward);

        var trashDecider = _pickUserToNominateStrategyFactory.ForTrashVoting();
        foreach (var trashMovie in votingResults.MoviesGoingByeBye)
        {
            var trashNominator = trashDecider.GetUserToNominate(trashMovie, assignNominationsUserContexts);
            var decade = message.MoviesWithVotes.Single(x => x.Movie.id == trashMovie.id).Movie.MovieCreationYear.ToDecade();
            list.Add(new ReadOnlyEmbeddedUserWithNominationAward { AwardMessage = "TODO 22", Decade = decade, User = trashNominator });
        }

        return list;
    }

    private readonly record struct ReadOnlyEmbeddedUserWithNominationAward(IReadOnlyEmbeddedUser User, string AwardMessage, Decade Decade) : IReadOnlyEmbeddedUserWithNominationAward;
}