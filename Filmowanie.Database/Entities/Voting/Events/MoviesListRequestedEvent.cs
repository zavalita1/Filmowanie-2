using Filmowanie.Abstractions;

namespace Filmowanie.Database.Entities.Voting.Events;

public record MoviesListRequestedEvent(VotingSessionId VotingSessionId) : IEvent { }