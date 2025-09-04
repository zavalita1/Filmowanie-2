using Filmowanie.Abstractions;

namespace Filmowanie.Database.Entities.Voting.Events;

public record RemoveVoteEvent(VotingSessionId VotingSessionId, EmbeddedMovie Movie, DomainUser User) : IEvent { }