using Filmowanie.Abstractions;
using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;

namespace Filmowanie.Database.Entities.Voting.Events;

public record AddVoteEvent(VotingSessionId VotingSessionId, EmbeddedMovie Movie, DomainUser User, VoteType VoteType) : IEvent;