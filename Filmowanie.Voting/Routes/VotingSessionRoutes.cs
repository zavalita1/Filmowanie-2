using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Voting.Interfaces;
using Filmowanie.Voting.Visitors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Routes;

internal sealed class VotingSessionRoutes : IVotingSessionRoutes
{
    private readonly ILogger<VotingSessionRoutes> _log;
    private readonly IUserIdentityVisitor _userIdentityVisitor;
    private readonly IGetCurrentVotingSessionVisitor _currentVotingSessionVisitor;

    public VotingSessionRoutes(ILogger<VotingSessionRoutes> log, IUserIdentityVisitor userIdentityVisitor, IGetCurrentVotingSessionVisitor currentVotingSessionVisitor)
    {
        _log = log;
        _userIdentityVisitor = userIdentityVisitor;
        _currentVotingSessionVisitor = currentVotingSessionVisitor;
    }

    public async Task<IResult> GetCurrentVotingSessionMovies(CancellationToken cancel)
    {
        var result = await OperationResultExtensions
            .Empty
            .Accept(_userIdentityVisitor)
            .AcceptAsync(_currentVotingSessionVisitor, cancel);

        throw new NotImplementedException();
    }
}