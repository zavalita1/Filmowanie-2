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

    public Task<Maybe<VotingSessionId>> MapAsync(Maybe<(string, DomainUser)> input, CancellationToken cancelToken) => input.AcceptAsync(MapAsync, _log, cancelToken);

    private async Task<Maybe<VotingSessionId>> MapAsync((string, DomainUser) input, CancellationToken cancelToken)
    {
        if (string.IsNullOrEmpty(input.Item1))
        {
            var currentUser = input.Item2.AsMaybe();
            var currentVoting = await _votingSessionService.GetCurrentVotingSessionIdAsync(currentUser, cancelToken);

            var result = await currentVoting.AcceptAsync(async (currentVotingSessionId, innerCt) =>
            {
                if (currentVotingSessionId.HasValue)
                    return currentVotingSessionId.Value.AsMaybe();

                return await _votingSessionService.GetLastVotingSessionIdAsync(currentUser, innerCt);
            }, _log, cancelToken);

            return result;
        }

        if (!Guid.TryParse(input.Item1, out var correlationId))
            return new Error<VotingSessionId>("Invalid id!", ErrorType.IncomingDataIssue);

        return new VotingSessionId(correlationId).AsMaybe();
    }
}