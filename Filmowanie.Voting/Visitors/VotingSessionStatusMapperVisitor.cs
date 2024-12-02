using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Voting.DTOs.Outgoing;
using Filmowanie.Voting.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Visitors;

internal sealed class MapperMapperVisitor : IVotingSessionStatusMapperVisitor, IAknowledgedMapperVisitor
{
    private readonly ILogger<MapperMapperVisitor> _log;

    public MapperMapperVisitor(ILogger<MapperMapperVisitor> log)
    {
        _log = log;
    }

    public OperationResult<VotingSessionStatusDto> Visit(OperationResult<VotingState> input)
    {
        var dto = new VotingSessionStatusDto(input.Result.ToString());
        return new OperationResult<VotingSessionStatusDto>(dto, null);
    }

    public OperationResult<AknowledgedDTO> Visit<T>(OperationResult<T> input)
    {
        return new OperationResult<AknowledgedDTO>(new AknowledgedDTO { Message = "OK"}, null);
    }

    public ILogger Log => _log;
}