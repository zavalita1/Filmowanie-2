using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Voting.DTOs.Outgoing;
using Filmowanie.Voting.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Mappers;

internal sealed class VotingStateMapper : IVotingStateMapper
{
    private readonly ILogger<VotingStateMapper> _log;

    public VotingStateMapper(ILogger<VotingStateMapper> log)
    {
        _log = log;
    }

    public Maybe<VotingSessionStatusDto> Map(Maybe<VotingSessionId?> input) => input.Accept(Map, _log);

    private static Maybe<VotingSessionStatusDto> Map(VotingSessionId? input)
    {
        var state = input == null ? VotingState.Results : VotingState.Voting;
        var result = new VotingSessionStatusDto(state.ToString(), input?.ToString());
        return result.AsMaybe();
    }
}