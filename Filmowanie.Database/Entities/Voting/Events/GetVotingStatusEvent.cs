using Filmowanie.Abstractions.DomainModels;

namespace Filmowanie.Database.Entities.Voting.Events;

public record GetVotingStatusEvent(VotingSessionId VotingSessionId) : IEvent { }