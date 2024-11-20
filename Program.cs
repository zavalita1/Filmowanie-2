using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Filmowanie;
using Filmowanie.Account.Constants;
using Filmowanie.Account.Interfaces;
using Filmowanie.Controllers;
using Filmowanie.Database.Contexts;
using Filmowanie.DTOs.Incoming;
using Filmowanie.Extensions;
using Filmowanie.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Environment = Filmowanie.Enums.Environment;
using IdentityDbContext = Filmowanie.Database.Contexts.IdentityDbContext;

var builder = WebApplication
    .CreateBuilder(args);

var environment = builder.Environment.IsDevelopment() ? Environment.Development : Environment.Production;

builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddSpaStaticFiles(so => so.RootPath = "ClientApp/build");
// TODO builder.Services.AddSignalR();

builder.Services
    .AddAuthentication(o =>
    {
        o.DefaultScheme = Schemes.Cookie;
    })
    .AddCookie(Schemes.Cookie, o =>
    {
        o.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };
    });

if (environment == Environment.Production)
{
    var keyVaultName = builder.Configuration["KeyVaultName"];
    var kvUri = $"https://{keyVaultName}.vault.azure.net";

    var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
    var keyVaultConfigurationProvider = new ConcurrentDictionary<string, string>();
    var keys = client.GetPropertiesOfSecrets().AsPages().ToArray().SelectMany(x => x.Values);
    await Parallel.ForEachAsync(keys, async (k, cancel) =>
    {
        var secret = await client.GetSecretAsync(k.Name, cancellationToken: cancel);
        keyVaultConfigurationProvider.AddOrUpdate(k.Name, secret.Value.Value, (_, _) => throw new NotSupportedException("Secret names must be unique!"));
    });
    builder.Configuration.AddInMemoryCollection(keyVaultConfigurationProvider);
}


var dbConnectionString = builder.Configuration["dbConnectionString"]!;
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseCosmos(connectionString: dbConnectionString, databaseName: "db-filmowanie2"));

var dataProtectionBuilder = builder.Services.AddDataProtection().SetApplicationName("filmowanie2");

if (environment != Environment.Development)
    dataProtectionBuilder.PersistKeysToDbContext<IdentityDbContext>();

builder.Services.RegisterPolicies();
builder.Services.RegisterCustomServices(builder.Configuration, environment);

// TODO database integration
var app = builder.Build();
if (environment != Environment.Development)
{
    app.UseSpaStaticFiles();
}

var apiGroup = app.MapGroup("api");
apiGroup.AddEndpointFilter<LoggingActionFilter>();

var accountRoutesBuilder = apiGroup.MapGroup("account");

accountRoutesBuilder.MapPost("login/code", ([FromServices] IAccountRoutes routes, [FromBody] LoginDto dto, CancellationToken ct) => routes.Login(dto, ct));
accountRoutesBuilder.MapPost("login/basic", ([FromServices] IAccountRoutes routes, [FromBody] BasicAuthLoginDto dto, CancellationToken ct) => routes.LoginBasic(dto, ct));
accountRoutesBuilder.MapPost("signup", ([FromServices] IAccountRoutes routes, [FromBody] BasicAuthLoginDto dto, CancellationToken ct) => routes.SignUp(dto, ct));
accountRoutesBuilder.MapPost("logout", ([FromServices] IAccountRoutes routes, CancellationToken ct) => routes.Logout(ct)).RequireAuthorization(Schemes.Cookie);
accountRoutesBuilder.MapGet("", ([FromServices] IAccountRoutes routes, CancellationToken ct) => routes.Get(ct)).RequireAuthorization();//.RequireAuthorization("admin");

app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/api"),
    then => then.UseSpa(spa =>
        {
            spa.Options.SourcePath = "ClientApp";

            if (environment != Environment.Production)
            {
                spa.UseReactDevelopmentServer(npmScript: "start");
            }
        }
    ));

//// TODO configure signalr hubs

// UsersCache.HydrateCache(); TODO

app.Run();