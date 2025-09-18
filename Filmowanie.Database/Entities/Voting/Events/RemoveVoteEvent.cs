using Filmowanie.Abstractions.DomainModels;

namespace Filmowanie.Database.Entities.Voting.Events;

public record RemoveVoteEvent(VotingSessionId VotingSessionId, EmbeddedMovie Movie, DomainUser User) : IEvent { }