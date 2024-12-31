using Microsoft.AspNetCore.Http;

namespace Filmowanie.Nomination.Interfaces;

internal interface INominationRoutes
{
    public Task<IResult> GetNominationsAsync(CancellationToken cancellationToken);

    public Task<IResult> GetNominationsFullDataAsync(CancellationToken cancellationToken);
    
    public Task<IResult> GetPostersAsync(string movieUrl, CancellationToken cancellationToken);
    
    public Task<IResult> DeleteMovieAsync(string movieId, CancellationToken cancellationToken);
    
    public Task<IResult> NominateAsync(DTOs.Incoming.NominationDTO dto, CancellationToken cancellationToken);
}