using Filmowanie.Voting.DTOs.Incoming;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Voting.Interfaces;

internal interface IVotingSessionRoutes
{
    public Task<IResult> GetCurrentVotingSessionMoviesAsync(CancellationToken cancel);
    public Task<IResult> VoteAsync(VoteDTO dto, CancellationToken cancel);
    public Task<IResult> GetVotingSessionStatus(CancellationToken cancel);
}

public interface IAdminVotingSessionRoutes
{
    public Task<IResult> NewVoting(CancellationToken cancel);
    public Task<IResult> ConcludeVoting(CancellationToken cancel);
}