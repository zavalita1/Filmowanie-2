using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Voting.Routes;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Retrievers;

public sealed class CurrentVotingStatusRetriever : ICurrentVotingStatusRetriever
{
    private readonly ILogger<CurrentVotingStatusRetriever> _log;
    private readonly IRequestClient<GetVotingStatusEvent> _requestClient;

    public CurrentVotingStatusRetriever(ILogger<CurrentVotingStatusRetriever> log, IRequestClient<GetVotingStatusEvent> requestClient)
    {
        _log = log;
        _requestClient = requestClient;
    }

    public Task<Maybe<VotingState>> GetCurrentVotingStatusAsync(Maybe<VotingSessionId?> input, CancellationToken cancel) => input.AcceptAsync(GetCurrentVotingStatusAsync, _log, cancel);

    public async Task<Maybe<VotingState>> GetCurrentVotingStatusAsync(VotingSessionId? votingSessionId, CancellationToken cancel)
    {
        if (!votingSessionId.HasValue)
            return VotingState.Results.AsMaybe();

        var msg = new GetVotingStatusEvent(votingSessionId.Value);
        var state = await _requestClient.GetResponse<CurrentVotingStatusResponse>(msg, cancel);

        return state.Message.State switch
        {
            "WaitingForNominations" => VotingState.Voting.AsMaybe(),
            "NominationsConcluded" => VotingState.Voting.AsMaybe(),
            "CalculatingResults" => VotingState.Results.AsMaybe(),
            _ => new Error<VotingState>($"Unknown voting state: {state}!", ErrorType.InvalidState)
        };
    }
}