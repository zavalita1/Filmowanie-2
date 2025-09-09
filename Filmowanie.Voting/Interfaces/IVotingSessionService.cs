using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.DTOs.Outgoing;

namespace Filmowanie.Voting.Interfaces;

internal interface IVotingSessionService : ICurrentVotingSessionIdAccessor
{
    public Task<Maybe<WinnerMetadata[]>> GetWinnersMetadataAsync(Maybe<(VotingMetadata[], TenantId)> input, CancellationToken cancelToken);

    public Task<Maybe<MovieVotingStandingsListDTO>> GetMovieVotingStandingsList(Maybe<TenantId> input, CancellationToken cancelToken);
}