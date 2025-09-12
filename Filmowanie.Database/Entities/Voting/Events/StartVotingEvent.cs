using Filmowanie.Abstractions;
using Filmowanie.Abstractions.DomainModels;

namespace Filmowanie.Database.Entities.Voting.Events;

public record StartVotingEvent(VotingSessionId VotingSessionId, EmbeddedMovie[] Movies, NominationData[] NominationsData, DateTime Created, TenantId TenantId) : IEvent;