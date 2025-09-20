using Filmowanie.Abstractions.Constants;
using Filmowanie.Account.DTOs.Incoming;
using Filmowanie.Account.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Filmowanie.Account.Extensions;

public static class RouteGroupBuilderExtensions
{
    public static RouteGroupBuilder RegisterAccountRoutes(this RouteGroupBuilder builder)
    {
        var accountRoutesBuilder = builder.MapGroup("account");

        accountRoutesBuilder.MapPost("login/code", ([FromServices] IAccountRoutes routes, [FromBody] LoginDto dto, CancellationToken ct) => routes.LoginAsync(dto, ct));
        accountRoutesBuilder.MapPost("login/basic", ([FromServices] IAccountRoutes routes, [FromBody] BasicAuthLoginDTO dto, CancellationToken ct) => routes.LoginBasicAsync(dto, ct));
        accountRoutesBuilder.MapPost("login/google", ([FromServices] IAccountRoutes routes, [FromBody] GoogleOAuthClientDTO dto, CancellationToken ct) => routes.LoginGoogleAsync(dto, ct));
        accountRoutesBuilder.MapPost("signup", ([FromServices] IAccountRoutes routes, [FromBody] BasicAuthLoginDTO dto, CancellationToken ct) => routes.SignUpAsync(dto, ct)).RequireAuthorization();
        accountRoutesBuilder.MapPost("logout", ([FromServices] IAccountRoutes routes, CancellationToken ct) => routes.LogoutAsync(ct)).RequireAuthorization();
        accountRoutesBuilder.MapGet("", ([FromServices] IAccountRoutes routes, CancellationToken ct) => routes.Get(ct));

        var accountAdminBuilder = builder.MapGroup("user");
        accountAdminBuilder.RequireAuthorization(Schemes.Admin);

        accountAdminBuilder.MapGet("all", ([FromServices] IAccountsAdministrationRoutes routes, CancellationToken ct) => routes.GetUsersAsync(ct));
        accountAdminBuilder.MapGet("{id}", ([FromServices] IAccountsAdministrationRoutes routes, [FromRoute] string id, CancellationToken ct) => routes.GetUserAsync(id, ct));
        accountAdminBuilder.MapPost("", ([FromServices] IAccountsAdministrationRoutes routes, [FromBody] UserDTO dto, CancellationToken ct) => routes.AddUserAsync(dto, ct));
       
        return builder;
    }
}