using Filmowanie.Abstractions.DomainModels;

namespace Filmowanie.Database.Entities.Voting.Events;

public record ResultsConfirmedEvent(VotingSessionId VotingSessionId, TenantId Tenant) : IEvent;