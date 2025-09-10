using Filmowanie.Abstractions.Maybe;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.DTOs.Outgoing;

namespace Filmowanie.Voting.Interfaces;

internal interface IVotingSessionsDTOMapper
{
    Maybe<VotingSessionsDTO> Map(Maybe<VotingMetadata[]> input);
}