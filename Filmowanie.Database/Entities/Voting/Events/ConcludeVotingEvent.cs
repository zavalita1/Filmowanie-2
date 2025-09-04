using Filmowanie.Abstractions;

namespace Filmowanie.Database.Entities.Voting.Events;

public record ConcludeVotingEvent(VotingSessionId VotingSessionId, TenantId Tenant) : IEvent;