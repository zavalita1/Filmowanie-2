using System.Globalization;
using Filmowanie.Abstractions;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.DTOs.Outgoing;
using Filmowanie.Voting.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Visitors;

internal sealed class HistoryDTOMapperVisitor : IHistoryDTOMapperVisitor
{
    private readonly ILogger<HistoryDTOMapperVisitor> _log;

    public HistoryDTOMapperVisitor(ILogger<HistoryDTOMapperVisitor> log)
    {
        _log = log;
    }

    public OperationResult<HistoryDTO> Visit(OperationResult<WinnerMetadata[]> input)
    {
        var entries = input
            .Result!
            .Select(x => new HistoryEntryDTO(x.Name, x.OriginalTitle, x.CreationYear, x.NominatedBy, x.Watched.ToString("d", new CultureInfo("pl"))))
            .ToArray();

        var result = new HistoryDTO(entries);

        return new OperationResult<HistoryDTO>(result, null);
    }

    public ILogger Log => _log;
}