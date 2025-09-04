using Filmowanie.Abstractions;

namespace Filmowanie.Database.Entities.Voting.Events;

public record NominationAddedEvent(VotingSessionId VotingSessionId, NominationData NominationData) : IEvent { }