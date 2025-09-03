using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Nomination.DTOs.Outgoing;
using Filmowanie.Nomination.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Nomination.Visitors;

internal sealed class NominationsVisitor : IGetNominationsVisitor, IGetNominationsDTOVisitor
{
    private readonly IRequestClient<NominationsRequested> _getNominationsRequestClient;
    private readonly ILogger<NominationsVisitor> _log;

    public NominationsVisitor(IRequestClient<NominationsRequested> getNominationsRequestClient, ILogger<NominationsVisitor> log)
    {
        _getNominationsRequestClient = getNominationsRequestClient;
        _log = log;
    }

    async Task<OperationResult<CurrentNominationsResponse>> IOperationAsyncVisitor<VotingSessionId?, CurrentNominationsResponse>.SignUp(OperationResult<VotingSessionId?> input, CancellationToken cancellationToken)
    {
        if (!input.Result.HasValue)
            return new OperationResult<CurrentNominationsResponse>(new CurrentNominationsResponse { CorrelationId = Guid.Empty, Nominations = [] }, null);

        var nominationsRequested = new NominationsRequested(input.Result.Value.CorrelationId);
        var nominations = await _getNominationsRequestClient.GetResponse<CurrentNominationsResponse>(nominationsRequested, cancellationToken);
        return new OperationResult<CurrentNominationsResponse>(nominations.Message, null);
    }

    public OperationResult<NominationsDataDTO> Visit(OperationResult<(CurrentNominationsResponse, DomainUser)> input)
    {
        var user = input.Result.Item2;
        var nominationDecades = input.Result.Item1.Nominations.Where(x => x.User.Id == user.Id).Select(x => x.Year.ToString()[1..]).ToArray();

        var result = new NominationsDataDTO { Nominations = nominationDecades };

        return new OperationResult<NominationsDataDTO>(result, null);
    }

    public ILogger Log => _log;
}