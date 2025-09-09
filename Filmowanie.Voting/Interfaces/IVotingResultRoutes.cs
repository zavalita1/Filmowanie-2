using Microsoft.AspNetCore.Http;

namespace Filmowanie.Voting.Interfaces;

internal interface IVotingResultRoutes
{
    public Task<IResult> GetResults(string votingSessionId, CancellationToken cancelToken);
    public Task<IResult> GetVotingSessionsList(CancellationToken cancelToken);
    public Task<IResult> GetWinnersList(CancellationToken cancelToken);

    public Task<IResult> GetLast10Standings(CancellationToken cancelToken);
}