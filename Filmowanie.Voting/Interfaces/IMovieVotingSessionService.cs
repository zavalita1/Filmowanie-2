using Filmowanie.Abstractions;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.DTOs.Outgoing;

namespace Filmowanie.Voting.Interfaces;

internal interface IMovieVotingSessionService
{
    Task<Maybe<MovieDTO[]>> GetCurrentlyVotedMoviesAsync(Maybe<(VotingSessionId, DomainUser)> input, CancellationToken cancellationToken);

    Task<Maybe<VotingResultDTO>> GetVotingResultsAsync(Maybe<(TenantId Tenant, VotingSessionId? VotingSessionId)> input, CancellationToken cancellationToken);

    public Task<Maybe<VotingMetadata[]>> GetVotingMetadataAsync(Maybe<TenantId> input, CancellationToken cancellationToken);
}