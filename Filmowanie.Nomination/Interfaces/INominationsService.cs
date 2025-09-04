using Filmowanie.Abstractions;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Nomination.DTOs.Incoming;
using Filmowanie.Nomination.DTOs.Outgoing;
using Filmowanie.Nomination.Models;

namespace Filmowanie.Nomination.Interfaces;

internal interface INominationsService
{
    Task<OperationResult<CurrentNominationsData>> GetNominationsAsync(OperationResult<VotingSessionId> maybeId, CancellationToken cancelToken);

    Task<OperationResult<AknowledgedNominationDTO>> RemoveMovieAsync(OperationResult<(string MovieId, DomainUser User, VotingSessionId VotingSessionId)> input,
        CancellationToken cancellationToken);

    Task<OperationResult<AknowledgedNominationDTO>> NominateAsync(OperationResult<(NominationDTO Dto, DomainUser User, CurrentNominationsData CurrentNominations)> input,
        CancellationToken cancellationToken);
}