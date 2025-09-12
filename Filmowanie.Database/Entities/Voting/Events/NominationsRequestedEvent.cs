using Filmowanie.Abstractions;
using Filmowanie.Abstractions.DomainModels;

namespace Filmowanie.Database.Entities.Voting.Events;

public record NominationsRequestedEvent(VotingSessionId VotingSessionId) : IEvent;