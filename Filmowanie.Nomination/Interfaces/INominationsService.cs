using Filmowanie.Abstractions;
using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Nomination.DTOs.Outgoing;
using Filmowanie.Nomination.Models;

namespace Filmowanie.Nomination.Interfaces;

internal interface INominationsService
{
    Task<Maybe<CurrentNominationsData>> GetNominationsAsync(Maybe<VotingSessionId> maybeId, CancellationToken cancelToken);

    Task<Maybe<AknowledgedNominationDTO>> ResetNominationAsync(Maybe<(string MovieId, DomainUser User, VotingSessionId VotingSessionId)> input,
        CancellationToken cancelToken);

    Task<Maybe<AknowledgedNominationDTO>> NominateAsync(Maybe<(IReadOnlyMovieEntity Movie, DomainUser User, VotingSessionId VotingSessionId)> input,
        CancellationToken cancelToken);
}