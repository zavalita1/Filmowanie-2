using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Voting.Helpers;
using Filmowanie.Voting.Interfaces;
using Filmowanie.Voting.Visitors;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Routes;

internal sealed class VotingSessionRoutes : IVotingSessionRoutes
{
    private readonly IBus _bus;
    private readonly ILogger<VotingSessionRoutes> _log;
    private readonly IUserIdentityVisitor _userIdentityVisitor;
    private readonly IGetCurrentVotingSessionVisitor _currentVotingSessionVisitor;
    private readonly IGetMoviesForVotingSessionVisitor _getMoviesForVotingSessionVisitor;
    private readonly IEnrichMoviesForVotingSessionWithPlaceholdersVisitor _enrichMoviesForVotingSessionWithPlaceholdersVisitor;

    public VotingSessionRoutes(IBus bus, ILogger<VotingSessionRoutes> log, IUserIdentityVisitor userIdentityVisitor, IGetCurrentVotingSessionVisitor currentVotingSessionVisitor, IGetMoviesForVotingSessionVisitor getMoviesForVotingSessionVisitor, IEnrichMoviesForVotingSessionWithPlaceholdersVisitor enrichMoviesForVotingSessionWithPlaceholdersVisitor)
    {
        _bus = bus;
        _log = log;
        _userIdentityVisitor = userIdentityVisitor;
        _currentVotingSessionVisitor = currentVotingSessionVisitor;
        _getMoviesForVotingSessionVisitor = getMoviesForVotingSessionVisitor;
        _enrichMoviesForVotingSessionWithPlaceholdersVisitor = enrichMoviesForVotingSessionWithPlaceholdersVisitor;
    }

    public async Task<IResult> GetCurrentVotingSessionMovies(CancellationToken cancel)
    {
        var userIdentity = OperationResultExtensions
            .Empty
            .Accept(_userIdentityVisitor);

        var votingSessionResult = await userIdentity.AcceptAsync(_currentVotingSessionVisitor, cancel);
        var result = await (await votingSessionResult
            .Merge(userIdentity)
            .AcceptAsync(_getMoviesForVotingSessionVisitor, cancel))
            .AcceptAsync(_enrichMoviesForVotingSessionWithPlaceholdersVisitor, cancel);

        return RoutesResultHelper.UnwrapOperationResult(result);
    }
}