using Filmowanie.Abstractions;
using Filmowanie.Abstractions.DomainModels;

namespace Filmowanie.Database.Entities.Voting.Events;

public record VoteAddedEvent(VotingSessionId VotingSessionId, EmbeddedMovie Movie, DomainUser User) : IEvent { }