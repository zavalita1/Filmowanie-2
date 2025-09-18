using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Voting.DTOs.Incoming;

namespace Filmowanie.Voting.Interfaces;

internal interface IVoteService
{
    Task<Maybe<VoidResult>> VoteAsync(Maybe<DomainUser> maybeCurrentUser, Maybe<VotingSessionId> maybeVotingId, Maybe<VoteDTO> maybeDto, CancellationToken cancelToken);
}