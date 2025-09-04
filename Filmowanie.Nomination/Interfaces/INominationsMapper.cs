using Filmowanie.Abstractions;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Nomination.DTOs.Outgoing;
using Filmowanie.Nomination.Models;

namespace Filmowanie.Nomination.Interfaces;

internal interface INominationsMapper
{
    Maybe<NominationsDataDTO> Map(Maybe<(CurrentNominationsData, DomainUser)> maybe);
    
    Task<Maybe<NominationsFullDataDTO>> EnrichNominationsAsync(Maybe<(NominationsDataDTO, DomainUser)> input, CancellationToken cancellationToken);
}