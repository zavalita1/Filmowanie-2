using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Nomination.DTOs.Outgoing;
using Filmowanie.Nomination.Models;

namespace Filmowanie.Nomination.Interfaces;

internal interface INominationsService
{
    Task<Maybe<CurrentNominationsData>> GetNominationsAsync(Maybe<VotingSessionId> maybeId, CancellationToken cancelToken);

    Task<Maybe<AknowledgedNominationDTO>> ResetNominationAsync(Maybe<string> maybeMovieId, Maybe<DomainUser> maybeUser, Maybe<VotingSessionId> maybeVotingId,
        CancellationToken cancelToken);

    Task<Maybe<AknowledgedNominationDTO>> NominateAsync(Maybe<IReadOnlyMovieEntity> maybeMovie, Maybe<DomainUser> maybeUser, Maybe<VotingSessionId> maybeCurrentVotingId,
        CancellationToken cancelToken);
}