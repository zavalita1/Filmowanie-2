using Microsoft.AspNetCore.Http;

namespace Filmowanie.Nomination.Interfaces;

internal interface INominationRoutes
{
    public Task<IResult> GetNominationsAsync(CancellationToken cancelToken);

    public Task<IResult> GetNominationsFullDataAsync(CancellationToken cancelToken);
    
    public Task<IResult> GetPostersAsync(string movieUrl, CancellationToken cancelToken);
    
    public Task<IResult> ResetNominationAsync(string movieId, CancellationToken cancelToken);
    
    public Task<IResult> NominateAsync(DTOs.Incoming.NominationDTO dto, CancellationToken cancelToken);
}