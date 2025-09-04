using Filmowanie.Abstractions;

namespace Filmowanie.Database.Entities.Voting.Events;

public record ErrorEvent(VotingSessionId VotingSessionId, string Message, string? CallStack) : IEvent;