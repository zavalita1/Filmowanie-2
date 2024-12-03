using Filmowanie.Abstractions;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Nomination.DTOs.Incoming;
using Filmowanie.Nomination.DTOs.Outgoing;

namespace Filmowanie.Nomination.Interfaces;

internal interface IGetNominationsVisitor : IOperationAsyncVisitor<VotingSessionId, CurrentNominationsResponse>;
internal interface IGetPostersVisitor : IOperationAsyncVisitor<string, PostersDTO>;
internal interface IGetNominationsDTOVisitor : IOperationVisitor<(CurrentNominationsResponse, DomainUser), NominationsDataDTO>;
internal interface INominationsCommandVisitor : IOperationAsyncVisitor<(NominationDTO Dto, DomainUser User, CurrentNominationsResponse CurrentNominations), AknowledgedNominationDTO>;