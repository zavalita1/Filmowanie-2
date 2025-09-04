using Filmowanie.Abstractions;

namespace Filmowanie.Database.Entities.Voting.Events;

public record NominationsRequestedEvent(VotingSessionId VotingSessionId) : IEvent;