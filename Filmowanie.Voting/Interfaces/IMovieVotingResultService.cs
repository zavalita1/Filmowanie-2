using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.DTOs.Outgoing;

namespace Filmowanie.Voting.Interfaces;

internal interface IMovieVotingResultService
{
    Task<Maybe<VotingResultDTO>> GetVotingResultsAsync(Maybe<(DomainUser CurrentUser, VotingSessionId? VotingSessionId)> input, CancellationToken cancelToken);

    public Task<Maybe<VotingMetadata[]>> GetVotingMetadataAsync(Maybe<TenantId> input, CancellationToken cancelToken);
}