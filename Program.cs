using System.Threading.Tasks;
using Filmowanie.Abstractions.Configuration;
using Filmowanie.Abstractions.Constants;
using Filmowanie.Database.Extensions;
using Filmowanie.Extensions;
using Filmowanie.Extensions.Initialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Environment = Filmowanie.Abstractions.Enums.Environment;

var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment.IsDevelopment() ? Environment.Development : Environment.Production;

builder.ConfigureLogging();

builder.Services.AddSignalR();

if (environment == Environment.Development)
    builder.Services.AddCors(o => o.AddPolicy("ViteLocalDevServer", p => p.WithOrigins(builder.Configuration["FrontendDevServer"]!)));

builder.Services
    .AddAuthentication(o => { o.DefaultScheme = Schemes.Cookie; })
    .AddCookie(Schemes.Cookie, o =>
    {
        o.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };
    });

if (environment == Environment.Production)
    await builder.SetupKeyVaultAsync();

builder.Services.AddMemoryCache();
builder.Services.RegisterPolicies();
builder.Services.RegisterCustomServices(builder.Configuration, environment);
builder.Services.RegisterDatabaseServices(builder.Configuration, environment);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<PushNotificationOptions>(builder.Configuration.GetSection("Vapid"));
builder.Services.ConfigureMassTransit(builder.Configuration);

var app = builder.Build();

app.ConfigureEndpoints(environment);

if (environment == Environment.Development)
    app.UseCors("ViteLocalDevServer");

await app.RunAsync();