using System.Globalization;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.DTOs.Outgoing;
using Filmowanie.Voting.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Mappers;

// TODO UTs
internal sealed class HistoryDtoMapper : IHistoryDtoMapper
{
    private readonly ILogger<HistoryDtoMapper> _log;

    public HistoryDtoMapper(ILogger<HistoryDtoMapper> log)
    {
        _log = log;
    }

    public Maybe<HistoryDTO> Map(Maybe<WinnerMetadata[]> input) => input.Accept(Map, _log);

    private static Maybe<HistoryDTO> Map(WinnerMetadata[] input)
    {
        var entries = input
            .Select(x => new HistoryEntryDTO(x.Name, x.OriginalTitle, x.CreationYear, x.NominatedBy, x.Watched.ToString("d", new CultureInfo("pl")), x.FilmwebUrl))
            .ToArray();

        var result = new HistoryDTO(entries);

        return result.AsMaybe();
    }
}