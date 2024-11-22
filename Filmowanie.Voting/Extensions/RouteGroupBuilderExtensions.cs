using Filmowanie.Voting.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Filmowanie.Voting.Extensions;

public static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder RegisterVotingRoutes(this RouteGroupBuilder builder)
    {
        var accountRoutesBuilder = builder.MapGroup("voting");

        accountRoutesBuilder.MapGet("current", ([FromServices] IVotingSessionRoutes routes, CancellationToken ct) => routes.GetCurrentVotingSessionMovies(ct));
       
        return builder;
    }
}