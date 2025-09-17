using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Maybe;

namespace Filmowanie.Voting.Routes;

public interface ICurrentVotingStatusRetriever
{
    Task<Maybe<VotingState>> GetCurrentVotingStatusAsync(Maybe<VotingSessionId?> input, CancellationToken cancel);
}
