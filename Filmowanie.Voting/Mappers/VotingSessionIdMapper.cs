using Filmowanie.Abstractions;
using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Voting.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Mappers;

// TODO UTs
internal sealed class VotingSessionIdMapper : IVotingSessionIdMapper
{
    private readonly ILogger<VotingSessionIdMapper> _log;
    private readonly IVotingSessionService _votingSessionService;

    public VotingSessionIdMapper(ILogger<VotingSessionIdMapper> log, IVotingSessionService votingSessionService)
    {
        _log = log;
        _votingSessionService = votingSessionService;
    }

    public Task<Maybe<VotingSessionId?>> MapAsync(Maybe<(string, DomainUser)> input, CancellationToken cancelToken) => input.AcceptAsync(MapAsync, _log, cancelToken);

    private async Task<Maybe<VotingSessionId?>> MapAsync((string, DomainUser) input, CancellationToken cancelToken)
    {
        if (string.IsNullOrEmpty(input.Item1))
            return await _votingSessionService.GetCurrentVotingSessionIdAsync(input.Item2.AsMaybe(), cancelToken);

        if (!Guid.TryParse(input.Item1, out var correlationId))
            return new Error<VotingSessionId?>("Invalid id!", ErrorType.IncomingDataIssue);

        return (new VotingSessionId(correlationId) as VotingSessionId?).AsMaybe();
    }
}