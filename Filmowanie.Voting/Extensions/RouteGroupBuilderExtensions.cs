using Filmowanie.Abstractions.Constants;
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
        accountRoutesBuilder.MapGet("state", ([FromServices] IVotingSessionRoutes routes, CancellationToken ct) => routes.GetVotingSessionStatus(ct));
        accountRoutesBuilder.MapPost("vote", ([FromServices] IVotingSessionRoutes routes, [FromBody] VoteDTO dto, CancellationToken ct) => routes.VoteAsync(dto, ct));

        var adminGroup = accountRoutesBuilder.MapGroup("admin").RequireAuthorization(Schemes.Admin);

        adminGroup.MapPost("start", ([FromServices] IAdminVotingSessionRoutes routes, CancellationToken ct) => routes.NewVoting(ct));
        adminGroup.MapPost("end", ([FromServices] IAdminVotingSessionRoutes routes, CancellationToken ct) => routes.ConcludeVoting(ct));
       
        return builder;
    }
}