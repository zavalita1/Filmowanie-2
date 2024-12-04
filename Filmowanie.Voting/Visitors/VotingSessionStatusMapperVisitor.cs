using System.Globalization;
using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.DTOs.Outgoing;
using Filmowanie.Voting.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Visitors;

internal sealed class VotingMapperVisitor : IVotingSessionStatusMapperVisitor, IAknowledgedMapperVisitor, IVotingSessionIdMapperVisitor, IVotingSessionsMapperVisitor
{
    private readonly ILogger<VotingMapperVisitor> _log;

    public VotingMapperVisitor(ILogger<VotingMapperVisitor> log)
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

    public OperationResult<VotingSessionId?> Visit(OperationResult<string> input)
    {
        if (string.IsNullOrEmpty(input.Result))
            return new OperationResult<VotingSessionId?>(null, null);

        if (!Guid.TryParse(input.Result, out var correlationId))
            return new OperationResult<VotingSessionId?>(default, new Error("Invalid id!", ErrorType.IncomingDataIssue));

        return new OperationResult<VotingSessionId?>(new VotingSessionId(correlationId), null);
    }

    public OperationResult<VotingSessionsDTO> Visit(OperationResult<VotingMetadata[]> input)
    {
        var dto = input.Result!.Select(x => new VotingSessionDTO(x.VotingSessionId, x.Concluded.ToString("D", new CultureInfo("pl")), x.Concluded.ToString("s")));
        var result = new VotingSessionsDTO(dto.ToArray());

        return new OperationResult<VotingSessionsDTO>(result, null);
    }

    public ILogger Log => _log;
}