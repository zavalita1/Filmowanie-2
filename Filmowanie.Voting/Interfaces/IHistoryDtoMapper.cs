using Filmowanie.Abstractions.Maybe;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.DTOs.Outgoing;

namespace Filmowanie.Voting.Interfaces;

internal interface IHistoryDtoMapper
{
    Maybe<HistoryDTO> Map(Maybe<WinnerMetadata[]> input);
}