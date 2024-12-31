using Filmowanie.Nomination.DTOs.Incoming;
using Filmowanie.Nomination.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Filmowanie.Nomination.Extensions;

public static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder RegisterNominationRoutes(this RouteGroupBuilder builder)
    {
        var accountRoutesBuilder = builder.MapGroup("nominations").RequireAuthorization();

        accountRoutesBuilder.MapGet("", ([FromServices] INominationRoutes routes, CancellationToken ct) => routes.GetNominationsAsync(ct));
        accountRoutesBuilder.MapGet("fullData", ([FromServices] INominationRoutes routes, CancellationToken ct) => routes.GetNominationsFullDataAsync(ct));
        accountRoutesBuilder.MapGet("posters", ([FromServices] INominationRoutes routes, [FromQuery] string movieUrl, CancellationToken ct) => routes.GetPostersAsync(movieUrl, ct));
        accountRoutesBuilder.MapDelete("", ([FromServices] INominationRoutes routes, [FromQuery] string movieId, CancellationToken ct) => routes.DeleteMovieAsync(movieId, ct));
        accountRoutesBuilder.MapPost("", ([FromServices] INominationRoutes routes, [FromBody] NominationDTO dto, CancellationToken ct) => routes.NominateAsync(dto, ct));

        return builder;
    }
}