using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.DTOs.Outgoing;

namespace Filmowanie.Voting.Interfaces;

internal interface IVotingSessionMapper
{
    Maybe<VotingState> Map(Maybe<VotingSessionId?> input);

    Maybe<VotingSessionId?> Map(Maybe<string> input);

    Maybe<VotingSessionsDTO> Map(Maybe<VotingMetadata[]> input);
    
    Maybe<HistoryDTO> Map(Maybe<WinnerMetadata[]> input);
}