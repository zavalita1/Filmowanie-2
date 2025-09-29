using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities.Voting.Events;

public record StartExtraVotingEvent(VotingSessionId VotingSessionId, IReadOnlyEmbeddedMovie[] ExtraVotingMovies) : IEvent;