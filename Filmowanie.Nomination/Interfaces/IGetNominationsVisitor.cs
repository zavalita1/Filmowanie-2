using Filmowanie.Abstractions;
using Filmowanie.Nomination.DTOs.Outgoing;

namespace Filmowanie.Nomination.Interfaces;

internal interface IGetNominationsVisitor : IOperationAsyncVisitor<(VotingSessionId, DomainUser), NominationsDataDTO>;