using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Voting.Helpers;
using Filmowanie.Voting.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Voting.Routes;

// TODO UTs
internal sealed class AdminVotingSessionRoutes : IAdminVotingSessionRoutes
{
    private readonly IVotingStateManager _votingStateManager;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ICurrentVotingSessionIdAccessor _votingSessionIdAccessor;

    public AdminVotingSessionRoutes(IVotingStateManager votingStateManager, ICurrentUserAccessor currentUserAccessor, ICurrentVotingSessionIdAccessor votingSessionIdAccessor)
    {
        _votingStateManager = votingStateManager;
        _currentUserAccessor = currentUserAccessor;
        _votingSessionIdAccessor = votingSessionIdAccessor;
    }

    public async Task<IResult> NewVoting(CancellationToken cancel)
    {
        var maybeCurrentUser = _currentUserAccessor.GetDomainUser(VoidResult.Void);
        var result = await _votingStateManager.StartNewVotingAsync(maybeCurrentUser, cancel);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> ConcludeVoting(CancellationToken cancel)
    {
        var maybeCurrentUser = _currentUserAccessor.GetDomainUser(VoidResult.Void);
        var maybeNullableVotingSessionId = await _votingSessionIdAccessor.GetCurrentVotingSessionIdAsync(maybeCurrentUser, cancel);
        var maybeVotingSessionId = _votingSessionIdAccessor.GetRequiredVotingSessionId(maybeNullableVotingSessionId);
        var result = await _votingStateManager.ConcludeVotingAsync(maybeVotingSessionId, maybeCurrentUser, cancel);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> ResumeVoting(CancellationToken cancel)
    {
        var maybeCurrentUser = _currentUserAccessor.GetDomainUser(VoidResult.Void);
        var maybeVotingSessionId = await _votingSessionIdAccessor.GetLastVotingSessionIdAsync(maybeCurrentUser, cancel);
        var result = await _votingStateManager.ResumeVotingAsync(maybeVotingSessionId, cancel);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }
}