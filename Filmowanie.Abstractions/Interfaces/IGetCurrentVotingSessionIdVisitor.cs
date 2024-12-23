using Filmowanie.Abstractions.OperationResult;

namespace Filmowanie.Abstractions.Interfaces;

public interface IGetCurrentVotingSessionIdVisitor : IOperationAsyncVisitor<DomainUser, VotingSessionId?>;
public interface IRequireCurrentVotingSessionIdVisitor : IOperationVisitor<VotingSessionId?, VotingSessionId>;