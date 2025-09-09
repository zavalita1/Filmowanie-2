using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Voting.DTOs.Incoming;

namespace Filmowanie.Voting.Interfaces;

internal interface IVoteService
{
    Task<Maybe<VoidResult>> VoteAsync(Maybe<(DomainUser, VotingSessionId, VoteDTO)> input, CancellationToken cancelToken);
}