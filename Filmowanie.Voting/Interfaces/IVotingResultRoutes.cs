using Microsoft.AspNetCore.Http;

namespace Filmowanie.Voting.Interfaces;

internal interface IVotingResultRoutes
{
    public Task<IResult> GetResults(string votingSessionId, CancellationToken cancellationToken);
    public Task<IResult> GetVotingSessionsList(CancellationToken cancellationToken);
    public Task<IResult> GetWinnersList(CancellationToken cancellationToken);

    public Task<IResult> GetLast10Standings(CancellationToken cancellationToken);
}