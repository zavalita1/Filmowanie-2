using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;

namespace Filmowanie.Database.Entities.Voting.Events;

public record AddMovieEvent(VotingSessionId VotingSessionId, EmbeddedMovie Movie, DomainUser User, Decade Decade) : IEvent;
