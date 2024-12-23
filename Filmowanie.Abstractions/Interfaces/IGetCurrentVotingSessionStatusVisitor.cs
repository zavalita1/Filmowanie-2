using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.OperationResult;

namespace Filmowanie.Abstractions.Interfaces;

public interface IGetCurrentVotingSessionStatusVisitor : IOperationAsyncVisitor<DomainUser, VotingState>;
