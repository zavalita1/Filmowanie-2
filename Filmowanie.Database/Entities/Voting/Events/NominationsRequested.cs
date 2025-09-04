using Filmowanie.Abstractions;

namespace Filmowanie.Database.Entities.Voting.Events;

public record NominationsRequested(VotingSessionId VotingSessionId) : IEvent;