using Filmowanie.Account.Constants;
using Filmowanie.Voting.DTOs.Incoming;
using Filmowanie.Voting.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Filmowanie.Voting.Extensions;

public static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder RegisterVotingRoutes(this RouteGroupBuilder builder)
    {
        var accountRoutesBuilder = builder.MapGroup("voting").RequireAuthorization();

        accountRoutesBuilder.MapGet("current", ([FromServices] IVotingSessionRoutes routes, CancellationToken ct) => routes.GetCurrentVotingSessionMoviesAsync(ct));
        accountRoutesBuilder.MapGet("state", ([FromServices] IVotingSessionRoutes routes, CancellationToken ct) => routes.GetCurrentVotingSessionMoviesAsync(ct));
        accountRoutesBuilder.MapPost("vote", ([FromServices] IVotingSessionRoutes routes, [FromBody] VoteDTO dto, CancellationToken ct) => routes.VoteAsync(dto, ct));
       
        return builder;
    }
}