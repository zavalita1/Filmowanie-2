using Filmowanie.Abstractions;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Entities.Voting.Events;

public record RemoveMovieEvent(VotingSessionId VotingSessionId, IReadOnlyEmbeddedMovie Movie, DomainUser User) : IEvent { }