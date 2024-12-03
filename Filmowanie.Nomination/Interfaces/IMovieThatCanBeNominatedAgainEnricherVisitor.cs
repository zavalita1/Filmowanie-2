using Filmowanie.Abstractions;
using Filmowanie.Nomination.DTOs.Outgoing;

namespace Filmowanie.Nomination.Interfaces;

internal interface IMovieThatCanBeNominatedAgainEnricherVisitor : IOperationAsyncVisitor<(NominationsDataDTO, DomainUser), NominationsFullDataDTO>;