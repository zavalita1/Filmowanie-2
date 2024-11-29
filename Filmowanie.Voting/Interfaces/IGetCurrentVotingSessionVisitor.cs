using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Voting.DTOs.Incoming;
using Filmowanie.Voting.DTOs.Outgoing;

namespace Filmowanie.Voting.Interfaces;

internal interface IGetCurrentVotingSessionVisitor : IOperationAsyncVisitor<DomainUser, VotingSessionId>;
internal interface IGetCurrentVotingSessionStatusVisitor : IOperationAsyncVisitor<DomainUser, VotingState>;
internal interface IVotingSessionStatusVisitor : IOperationVisitor<VotingState, VotingSessionStatusDto>;
internal interface IVoteVisitor : IOperationAsyncVisitor<(DomainUser, VotingSessionId, VoteDTO), object>;