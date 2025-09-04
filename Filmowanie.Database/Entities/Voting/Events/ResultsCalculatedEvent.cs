using Filmowanie.Abstractions;

namespace Filmowanie.Database.Entities.Voting.Events;

public record ResultsCalculatedEvent(VotingSessionId VotingSessionId) : IEvent;