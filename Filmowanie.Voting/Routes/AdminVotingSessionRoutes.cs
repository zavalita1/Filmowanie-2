using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Voting.Helpers;
using Filmowanie.Voting.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Voting.Routes;

// TODO UTs
internal sealed class AdminVotingSessionRoutes : IAdminVotingSessionRoutes
{
    private readonly IVotingStateManager votingStateManager;
    private readonly ICurrentUserAccessor currentUserAccessor;
    private readonly ICurrentVotingSessionIdAccessor votingSessionIdAccessor;

    public AdminVotingSessionRoutes(IVotingStateManager votingStateManager, ICurrentUserAccessor currentUserAccessor, ICurrentVotingSessionIdAccessor votingSessionIdAccessor)
    {
        this.votingStateManager = votingStateManager;
        this.currentUserAccessor = currentUserAccessor;
        this.votingSessionIdAccessor = votingSessionIdAccessor;
    }

    public async Task<IResult> NewVoting(CancellationToken cancel)
    {
        var maybeCurrentUser = this.currentUserAccessor.GetDomainUser(VoidResult.Void);
        var result = await this.votingStateManager.StartNewVotingAsync(maybeCurrentUser, cancel);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> ConcludeVoting(CancellationToken cancel)
    {
        var maybeCurrentUser = this.currentUserAccessor.GetDomainUser(VoidResult.Void);
        var maybeNullableVotingSessionId = await this.votingSessionIdAccessor.GetCurrentVotingSessionIdAsync(maybeCurrentUser, cancel);
        var maybeVotingSessionId = this.votingSessionIdAccessor.GetRequiredVotingSessionId(maybeNullableVotingSessionId);
        var result = await this.votingStateManager.ConcludeVotingAsync(maybeVotingSessionId, maybeCurrentUser, cancel);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> ResumeVoting(CancellationToken cancel)
    {
        var maybeCurrentUser = this.currentUserAccessor.GetDomainUser(VoidResult.Void);
        var maybeVotingSessionId = await this.votingSessionIdAccessor.GetLastVotingSessionIdAsync(maybeCurrentUser, cancel);
        var result = await this.votingStateManager.ResumeVotingAsync(maybeVotingSessionId, cancel);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }
}