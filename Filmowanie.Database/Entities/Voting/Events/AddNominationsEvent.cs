using Filmowanie.Abstractions;

namespace Filmowanie.Database.Entities.Voting.Events;

public record AddNominationsEvent(VotingSessionId VotingSessionId, NominationData[] NominationsData) : IEvent { }