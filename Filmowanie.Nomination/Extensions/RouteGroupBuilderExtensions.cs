using Filmowanie.Nomination.Interfaces;
using Filmowanie.Nomination.Routes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Filmowanie.Nomination.Extensions;

public static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder RegisterNominationRoutes(this RouteGroupBuilder builder)
    {
        var accountRoutesBuilder = builder.MapGroup("nominations").RequireAuthorization();

        accountRoutesBuilder.MapGet("", ([FromServices] INominationRoutes routes, CancellationToken ct) => routes.GetNominations(ct));

        return builder;
    }
}