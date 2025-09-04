using Filmowanie.Abstractions;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Nomination.DTOs.Incoming;
using Filmowanie.Nomination.DTOs.Outgoing;
using Filmowanie.Nomination.Models;

namespace Filmowanie.Nomination.Interfaces;

internal interface INominationsService
{
    Task<Maybe<CurrentNominationsData>> GetNominationsAsync(Maybe<VotingSessionId> maybeId, CancellationToken cancelToken);

    Task<Maybe<AknowledgedNominationDTO>> RemoveMovieAsync(Maybe<(string MovieId, DomainUser User, VotingSessionId VotingSessionId)> input,
        CancellationToken cancellationToken);

    Task<Maybe<AknowledgedNominationDTO>> NominateAsync(Maybe<(NominationDTO Dto, DomainUser User, CurrentNominationsData CurrentNominations)> input,
        CancellationToken cancellationToken);
}