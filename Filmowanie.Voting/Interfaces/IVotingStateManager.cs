using Filmowanie.Abstractions;
using Filmowanie.Abstractions.OperationResult;

namespace Filmowanie.Voting.Interfaces;

internal interface IVotingStateManager
{
    Task<Maybe<VoidResult>> ConcludeVotingAsync(Maybe<(VotingSessionId, DomainUser)> input, CancellationToken cancellationToken);

    Task<Maybe<VotingSessionId>> StartNewVotingAsync(Maybe<DomainUser> input, CancellationToken cancellationToken);
}