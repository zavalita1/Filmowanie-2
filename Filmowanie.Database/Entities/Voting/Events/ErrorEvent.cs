using Filmowanie.Abstractions.DomainModels;

namespace Filmowanie.Database.Entities.Voting.Events;

public record ErrorEvent(VotingSessionId VotingSessionId, string Message, string? CallStack) : IEvent;