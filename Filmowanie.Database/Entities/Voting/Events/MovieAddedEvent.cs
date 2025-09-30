using Filmowanie.Abstractions.DomainModels;

namespace Filmowanie.Database.Entities.Voting.Events;

public record MovieAddedEvent(VotingSessionId VotingSessionId, EmbeddedMovie Movie) : IEvent;