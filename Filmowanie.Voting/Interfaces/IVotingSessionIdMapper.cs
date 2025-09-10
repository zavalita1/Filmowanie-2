using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Maybe;

namespace Filmowanie.Voting.Interfaces;

internal interface IVotingSessionIdMapper
{
    Task<Maybe<VotingSessionId?>> MapAsync(Maybe<(string, DomainUser)> input, CancellationToken cancelToken);
}