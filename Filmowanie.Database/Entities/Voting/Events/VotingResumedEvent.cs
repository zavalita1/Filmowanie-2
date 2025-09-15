using Filmowanie.Abstractions.DomainModels;

namespace Filmowanie.Database.Entities.Voting.Events;

public record ResumeVotingEvent(VotingSessionId VotingSessionId) : IEvent;