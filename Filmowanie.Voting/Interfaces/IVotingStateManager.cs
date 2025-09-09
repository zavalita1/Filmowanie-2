using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Maybe;

namespace Filmowanie.Voting.Interfaces;

internal interface IVotingStateManager
{
    Task<Maybe<VoidResult>> ConcludeVotingAsync(Maybe<(VotingSessionId, DomainUser)> input, CancellationToken cancelToken);

    Task<Maybe<VotingSessionId>> StartNewVotingAsync(Maybe<DomainUser> input, CancellationToken cancelToken);
}