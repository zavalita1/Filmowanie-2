using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.DomainModels;

namespace Filmowanie.Voting.Interfaces;

internal interface INominationsRetriever
{
    List<IReadOnlyEmbeddedUserWithNominationAward> GetNominations(Dictionary<IReadOnlyEmbeddedUser, PickUserToNominateContext> assignNominationsUserContexts, VotingConcludedEvent message, VotingResults votingResults);
}