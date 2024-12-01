using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Voting.DTOs.Outgoing;
using Filmowanie.Voting.Interfaces;

namespace Filmowanie.Voting.Visitors;

internal sealed class MapperMapperVisitor : IVotingSessionStatusMapperVisitor, IAknowledgedMapperVisitor
{
    public OperationResult<VotingSessionStatusDto> Visit(OperationResult<VotingState> input)
    {
        var dto = new VotingSessionStatusDto(input.Result.ToString());
        return new OperationResult<VotingSessionStatusDto>(dto, null);
    }

    public OperationResult<AknowledgedDTO> Visit<T>(OperationResult<T> input)
    {
        return new OperationResult<AknowledgedDTO>();
    }
}