using Filmowanie.Abstractions;

namespace Filmowanie.Voting.Interfaces;

internal interface IGetCurrentVotingSessionVisitor : IOperationAsyncVisitor<DomainUser, VotingSessionId>;