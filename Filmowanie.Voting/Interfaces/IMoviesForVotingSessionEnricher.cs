using Filmowanie.Abstractions;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Voting.DTOs.Outgoing;

namespace Filmowanie.Voting.Interfaces;

internal interface IMoviesForVotingSessionEnricher
{
    public Task<Maybe<MovieDTO[]>> EnrichWithPlaceholdersAsync(Maybe<(MovieDTO[], VotingSessionId)> movies, CancellationToken cancellationToken);
}
