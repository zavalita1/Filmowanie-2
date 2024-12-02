using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Voting.DTOs.Incoming;
using Filmowanie.Voting.DTOs.Outgoing;

namespace Filmowanie.Voting.Interfaces;


internal interface IVotingSessionStatusMapperVisitor : IOperationVisitor<VotingState, VotingSessionStatusDto>;
internal interface IAknowledgedMapperVisitor : IOperationVisitor<AknowledgedDTO>;
internal interface IVoteVisitor : IOperationAsyncVisitor<(DomainUser, VotingSessionId, VoteDTO), object>;
internal interface IStartNewVotingVisitor : IOperationAsyncVisitor<DomainUser, VotingSessionId>;
internal interface IConcludeVotingVisitor : IOperationAsyncVisitor<(VotingSessionId, DomainUser), object>;