using Microsoft.AspNetCore.Http;

namespace Filmowanie.Nomination.Interfaces;

internal interface INominationRoutes
{
    Task<IResult> GetNominations(CancellationToken cancellationToken);
}