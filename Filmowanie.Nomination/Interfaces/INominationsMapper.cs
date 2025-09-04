using Filmowanie.Abstractions;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Nomination.DTOs.Outgoing;
using Filmowanie.Nomination.Models;

namespace Filmowanie.Nomination.Interfaces;

internal interface INominationsMapper
{
    OperationResult<NominationsDataDTO> Map(OperationResult<(CurrentNominationsData, DomainUser)> maybe);
    
    Task<OperationResult<NominationsFullDataDTO>> EnrichNominationsAsync(OperationResult<(NominationsDataDTO, DomainUser)> input, CancellationToken cancellationToken);
}