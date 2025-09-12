using Filmowanie.Abstractions;
using Filmowanie.Abstractions.DomainModels;

namespace Filmowanie.Database.Entities.Voting.Events;

public record ResultsCalculatedEvent(VotingSessionId VotingSessionId) : IEvent;