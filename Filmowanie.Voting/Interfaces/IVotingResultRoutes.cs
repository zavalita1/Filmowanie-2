using Microsoft.AspNetCore.Http;

namespace Filmowanie.Voting.Interfaces;

internal interface IVotingResultRoutes
{
    Task<IResult> GetResults(string votingSessionId, CancellationToken cancellationToken);
    Task<IResult> GetVotingSessionsList(CancellationToken cancellationToken);
    Task<IResult> GetWinnersList(CancellationToken cancellationToken);
}