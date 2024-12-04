using Filmowanie.Voting.DTOs.Outgoing;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Voting.Interfaces;

internal interface IVotingResultRoutes
{
    Task<IResult> GetResults(string votingSessionId, CancellationToken cancellationToken);
    Task<IResult> GetVotingSessionsList(CancellationToken cancellationToken);
}