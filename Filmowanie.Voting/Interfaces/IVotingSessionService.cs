using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.DTOs.Outgoing;

namespace Filmowanie.Voting.Interfaces;

internal interface IVotingSessionService : ICurrentVotingSessionIdAccessor
{
    Task<Maybe<IReadOnlyVotingResult?>> GetCurrentVotingSession(Maybe<DomainUser> maybeCurrentUser, CancellationToken cancellationToken);

    public Task<Maybe<WinnerMetadata[]>> GetWinnersMetadataAsync(Maybe<(VotingMetadata[], TenantId)> input, CancellationToken cancellationToken);

    public Task<Maybe<MovieVotingStandingsListDTO>> GetMovieVotingStandingsList(Maybe<TenantId> input, CancellationToken cancellationToken);
}