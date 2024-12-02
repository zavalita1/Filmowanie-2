using Filmowanie.Abstractions.Enums;

namespace Filmowanie.Abstractions.Interfaces;

public interface IGetCurrentVotingSessionStatusVisitor : IOperationAsyncVisitor<DomainUser, VotingState>;