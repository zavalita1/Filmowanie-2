using Microsoft.AspNetCore.Http;

namespace Filmowanie.Voting.Interfaces;

internal interface IVotingSessionRoutes
{
    Task<IResult> GetCurrentVotingSessionMovies(CancellationToken cancel);
}