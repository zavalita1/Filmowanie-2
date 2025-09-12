using Filmowanie.Abstractions;
using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Nomination.DTOs.Outgoing;
using Filmowanie.Nomination.Models;

namespace Filmowanie.Nomination.Interfaces;

internal interface INominationsMapper
{
    Maybe<NominationsDataDTO> Map(Maybe<(CurrentNominationsData, DomainUser)> maybe);
}