namespace Filmowanie.Abstractions.Interfaces;

public interface IGetCurrentVotingSessionIdVisitor : IOperationAsyncVisitor<DomainUser, VotingSessionId>;