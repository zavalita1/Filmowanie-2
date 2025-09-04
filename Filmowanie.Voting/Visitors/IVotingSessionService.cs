using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Voting.Visitors;

internal interface IVotingSessionService : ICurrentVotingSessionIdAccessor
{
    Task<OperationResult<IReadOnlyVotingResult?>> GetCurrentVotingSession(OperationResult<DomainUser> maybeCurrentUser, CancellationToken cancellationToken);
}