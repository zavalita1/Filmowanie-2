using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Voting.Routes;
using Filmowanie.Voting.Sagas;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Retrievers;

public sealed class CurrentVotingStatusRetriever : ICurrentVotingStatusRetriever
{
    private readonly ILogger<CurrentVotingStatusRetriever> log;
    private readonly IRequestClient<GetVotingStatusEvent> requestClient;

    public CurrentVotingStatusRetriever(ILogger<CurrentVotingStatusRetriever> log, IRequestClient<GetVotingStatusEvent> requestClient)
    {
        this.log = log;
        this.requestClient = requestClient;
    }

    public Task<Maybe<VotingState>> GetCurrentVotingStatusAsync(Maybe<VotingSessionId?> input, CancellationToken cancel) => input.AcceptAsync(GetCurrentVotingStatusAsync, this.log, cancel);

    public async Task<Maybe<VotingState>> GetCurrentVotingStatusAsync(VotingSessionId? votingSessionId, CancellationToken cancel)
    {
        if (!votingSessionId.HasValue)
            return VotingState.Results.AsMaybe();

        var msg = new GetVotingStatusEvent(votingSessionId.Value);
        var state = await this.requestClient.GetResponse<CurrentVotingStatusResponse>(msg, cancel);

        return state.Message.State switch
        {
            nameof(VotingStateMachine.WaitingForNominations) => VotingState.Voting.AsMaybe(),
            nameof(VotingStateMachine.NominationsConcluded) => VotingState.Voting.AsMaybe(),
            nameof(VotingStateMachine.ExtraVoting) => VotingState.ExtraVoting.AsMaybe(),
            nameof(VotingStateMachine.CalculatingResults) => VotingState.Results.AsMaybe(),
            nameof(VotingStateMachine.CalculatingExtraResults) => VotingState.Results.AsMaybe(),
            nameof(VotingStateMachine.Final) => VotingState.Results.AsMaybe(),
            _ => new Error<VotingState>($"Unknown voting state: {state}!", ErrorType.InvalidState)
        };
    }
}