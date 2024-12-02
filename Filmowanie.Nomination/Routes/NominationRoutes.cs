using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Nomination.Helpers;
using Filmowanie.Nomination.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Nomination.Routes;

internal sealed class NominationRoutes : INominationRoutes
{
    private readonly IUserIdentityVisitor _userIdentityVisitor;
    private readonly IGetNominationsVisitor _getNominationsVisitor;
    private readonly IGetCurrentVotingSessionIdVisitor _currentVotingSessionIdVisitor;

    public NominationRoutes(IUserIdentityVisitor userIdentityVisitor, IGetNominationsVisitor getNominationsVisitor, IGetCurrentVotingSessionIdVisitor currentVotingSessionIdVisitor)
    {
        _userIdentityVisitor = userIdentityVisitor;
        _getNominationsVisitor = getNominationsVisitor;
        _currentVotingSessionIdVisitor = currentVotingSessionIdVisitor;
    }

    public async Task<IResult> GetNominations(CancellationToken cancellationToken)
    {
        var identityResult = OperationResultExtensions.Empty.Accept(_userIdentityVisitor);

        var result = await (await identityResult
            .AcceptAsync(_currentVotingSessionIdVisitor, cancellationToken))
            .Merge(identityResult)
            .AcceptAsync(_getNominationsVisitor, cancellationToken);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }
}