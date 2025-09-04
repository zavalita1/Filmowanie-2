using Filmowanie.Abstractions;

namespace Filmowanie.Database.Entities.Voting.Events;

public record VoteAddedEvent(VotingSessionId VotingSessionId, EmbeddedMovie Movie, DomainUser User) : IEvent { }