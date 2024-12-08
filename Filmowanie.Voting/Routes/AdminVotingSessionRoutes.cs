using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Voting.Helpers;
using Filmowanie.Voting.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Voting.Routes;

internal sealed class AdminVotingSessionRoutes : IAdminVotingSessionRoutes
{
    private readonly IConcludeVotingVisitor _concludeVotingVisitor;
    private readonly IStartNewVotingVisitor _startNewVotingVisitor;
    private readonly IUserIdentityVisitor _userIdentityVisitor;
    private readonly IGetCurrentVotingSessionIdVisitor _currentVotingSessionIdStatusVisitor;
    private readonly IRequireCurrentVotingSessionIdVisitor _requireCurrentVotingSessionIdVisitor;

    public AdminVotingSessionRoutes(IConcludeVotingVisitor concludeVotingVisitor, IStartNewVotingVisitor startNewVotingVisitor, IUserIdentityVisitor userIdentityVisitor, IGetCurrentVotingSessionIdVisitor currentVotingSessionIdStatusVisitor, IRequireCurrentVotingSessionIdVisitor requireCurrentVotingSessionIdVisitor)
    {
        _concludeVotingVisitor = concludeVotingVisitor;
        _startNewVotingVisitor = startNewVotingVisitor;
        _userIdentityVisitor = userIdentityVisitor;
        _currentVotingSessionIdStatusVisitor = currentVotingSessionIdStatusVisitor;
        _requireCurrentVotingSessionIdVisitor = requireCurrentVotingSessionIdVisitor;
    }

    public async Task<IResult> NewVoting(CancellationToken cancel)
    {
        var result = await OperationResultExtensions
            .Empty
            .Accept(_userIdentityVisitor)
            .AcceptAsync(_startNewVotingVisitor, cancel);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> ConcludeVoting(CancellationToken cancel)
    {
        var user = OperationResultExtensions
            .Empty
            .Accept(_userIdentityVisitor);

        var result = await (await user
            .AcceptAsync(_currentVotingSessionIdStatusVisitor, cancel))
            .Accept(_requireCurrentVotingSessionIdVisitor)
            .Merge(user)
            .AcceptAsync(_concludeVotingVisitor, cancel);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }
}