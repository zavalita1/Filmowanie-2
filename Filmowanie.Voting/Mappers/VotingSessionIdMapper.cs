using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Voting.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Mappers;

// TODO UTs
internal sealed class VotingSessionIdMapper : IVotingSessionIdMapper
{
    private readonly ILogger<VotingSessionIdMapper> log;
    private readonly IVotingSessionService votingSessionService;

    public VotingSessionIdMapper(ILogger<VotingSessionIdMapper> log, IVotingSessionService votingSessionService)
    {
        this.log = log;
        this.votingSessionService = votingSessionService;
    }

    public Task<Maybe<VotingSessionId>> MapAsync(Maybe<(string, DomainUser)> input, CancellationToken cancelToken) => input.AcceptAsync(MapAsync, this.log, cancelToken);

    private async Task<Maybe<VotingSessionId>> MapAsync((string, DomainUser) input, CancellationToken cancelToken)
    {
        if (string.IsNullOrEmpty(input.Item1))
        {
            var currentUser = input.Item2.AsMaybe();
            var currentVoting = await this.votingSessionService.GetCurrentVotingSessionIdAsync(currentUser, cancelToken);

            var result = await currentVoting.AcceptAsync(async (currentVotingSessionId, innerCt) =>
            {
                if (currentVotingSessionId.HasValue)
                    return currentVotingSessionId.Value.AsMaybe();

                return await this.votingSessionService.GetLastVotingSessionIdAsync(currentUser, innerCt);
            }, this.log, cancelToken);

            return result;
        }

        if (!Guid.TryParse(input.Item1, out var correlationId))
            return new Error<VotingSessionId>("Invalid id!", ErrorType.IncomingDataIssue);

        return new VotingSessionId(correlationId).AsMaybe();
    }
}