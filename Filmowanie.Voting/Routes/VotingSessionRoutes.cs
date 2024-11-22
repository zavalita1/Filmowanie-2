using Filmowanie.Voting.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Routes;

internal sealed class VotingSessionRoutes : IVotingSessionRoutes
{
    private readonly ILogger<VotingSessionRoutes> _log;

    public VotingSessionRoutes(ILogger<VotingSessionRoutes> log)
    {
        _log = log;
    }

    public async Task<IResult> GetCurrentVotingSessionMovies(CancellationToken cancel)
    {
        throw new NotSupportedException();
    }
}