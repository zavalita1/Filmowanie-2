using Filmowanie.Abstractions;
using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Voting.DTOs.Outgoing;
using Filmowanie.Voting.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Mappers;

// TODO UTs
internal sealed class VotingStateMapper : IVotingStateMapper
{
    private readonly ILogger<VotingStateMapper> _log;

    public VotingStateMapper(ILogger<VotingStateMapper> log)
    {
        _log = log;
    }

    public Maybe<VotingSessionStatusDto> Map(Maybe<(VotingState, VotingSessionId?)> input) => input.Accept(Map, _log);

    private static Maybe<VotingSessionStatusDto> Map((VotingState, VotingSessionId?) input)
    {
        var result = new VotingSessionStatusDto(input.Item1.ToString(), input.Item2?.ToString());
        return result.AsMaybe();
    }
}