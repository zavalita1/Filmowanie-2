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

        accountRoutesBuilder.MapPost("login/code", ([FromServices] IAccountRoutes routes, [FromBody] LoginDto dto, CancellationToken ct) => routes.Login(dto, ct));
        accountRoutesBuilder.MapPost("login/basic", ([FromServices] IAccountRoutes routes, [FromBody] BasicAuthLoginDTO dto, CancellationToken ct) => routes.LoginBasic(dto, ct));
        accountRoutesBuilder.MapPost("signup", ([FromServices] IAccountRoutes routes, [FromBody] BasicAuthLoginDTO dto, CancellationToken ct) => routes.SignUp(dto, ct)).RequireAuthorization();
        accountRoutesBuilder.MapPost("logout", ([FromServices] IAccountRoutes routes, CancellationToken ct) => routes.Logout(ct)).RequireAuthorization();
        accountRoutesBuilder.MapGet("", ([FromServices] IAccountRoutes routes, CancellationToken ct) => routes.Get(ct));

        var accountAdminBuilder = builder.MapGroup("user");
        accountAdminBuilder.RequireAuthorization(Schemes.Admin);

        accountAdminBuilder.MapGet("all", ([FromServices] IAccountsAministrationRoutes routes, CancellationToken ct) => routes.GetUsers(ct));
        accountAdminBuilder.MapGet("{id}", ([FromServices] IAccountsAministrationRoutes routes, [FromRoute] string id, CancellationToken ct) => routes.GetUser(id, ct));
        accountAdminBuilder.MapPost("", ([FromServices] IAccountsAministrationRoutes routes, [FromBody] UserDTO dto, CancellationToken ct) => routes.AddUser(dto, ct));
       
        return builder;
    }
}