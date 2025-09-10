using Filmowanie.Abstractions.Maybe;
using Filmowanie.Nomination.DTOs.Outgoing;

namespace Filmowanie.Nomination.Interfaces;

internal interface INominationsEnricher
{
    Task<Maybe<NominationsFullDataDTO>> EnrichNominationsAsync(Maybe<NominationsDataDTO> input, CancellationToken cancelToken);
}