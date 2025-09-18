using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Voting.DTOs.Outgoing;

namespace Filmowanie.Voting.Interfaces;

internal interface IMoviesForVotingSessionEnricher
{
    public Task<Maybe<MovieDTO[]>> EnrichWithPlaceholdersAsync(Maybe<MovieDTO[]> movies, Maybe<VotingSessionId> maybeVotingId, CancellationToken cancelToken);
}
