using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Voting.Interfaces;

public interface IPickUserToNominateContextRetriever
{
    Dictionary<IReadOnlyEmbeddedUser, PickUserToNominateContext> GetPickUserToNominateContexts(IReadOnlyVotingResult[] lastVotingResults, EmbeddedMovieWithNominationContext[] moviesAdded, VotingConcludedEvent message);
}