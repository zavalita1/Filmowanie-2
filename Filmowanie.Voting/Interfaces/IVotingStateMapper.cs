using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Voting.DTOs.Outgoing;

namespace Filmowanie.Voting.Interfaces;

internal interface IVotingStateMapper
{
    Maybe<VotingSessionStatusDto> Map(Maybe<VotingState> maybeVotingState, Maybe<VotingSessionId?> maybeVotingId);
}