using Filmowanie.Abstractions.DomainModels;

namespace Filmowanie.Database.Entities.Voting.Events;

public record MoviesListRequestedEvent(VotingSessionId VotingSessionId) : IEvent { }
