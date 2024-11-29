using Filmowanie.Voting.DTOs.Incoming;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Voting.Interfaces;

internal interface IVotingSessionRoutes
{
    Task<IResult> GetCurrentVotingSessionMoviesAsync(CancellationToken cancel);

    Task<IResult> VoteAsync(VoteDTO dto, CancellationToken cancel);
}