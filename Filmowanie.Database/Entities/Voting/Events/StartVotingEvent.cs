using Filmowanie.Abstractions;

namespace Filmowanie.Database.Entities.Voting.Events;

public record StartVotingEvent(VotingSessionId VotingSessionId, EmbeddedMovie[] Movies, NominationData[] NominationsData, DateTime Created, TenantId TenantId) : IEvent;