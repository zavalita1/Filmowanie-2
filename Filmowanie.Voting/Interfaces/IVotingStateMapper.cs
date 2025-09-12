using Filmowanie.Abstractions;
using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Voting.DTOs.Outgoing;

namespace Filmowanie.Voting.Interfaces;

internal interface IVotingStateMapper
{
    Maybe<VotingSessionStatusDto> Map(Maybe<VotingSessionId?> input);
}