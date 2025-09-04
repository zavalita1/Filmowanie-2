using Filmowanie.Abstractions;

namespace Filmowanie.Database.Entities.Voting.Events;

public record VotingStartingEvent(VotingSessionId VotingSessionId) : IEvent;