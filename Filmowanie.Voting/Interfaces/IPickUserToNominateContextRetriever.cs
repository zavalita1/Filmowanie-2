using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Voting.Interfaces;

public interface IPickUserToNominateContextRetriever
{
    Dictionary<IReadOnlyEmbeddedUser, PickUserToNominateContext> GetPickUserToNominateContexts(IReadOnlyVotingResult[] lastVotingResults, IReadOnlyEmbeddedMovieWithNominationContext[] moviesAdded, VotingConcludedEvent message);
}