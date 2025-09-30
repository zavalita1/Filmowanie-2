using System.Threading.Tasks;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Filmowanie.Abstractions.Constants;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Database.Extensions;
using Filmowanie.Extensions;
using Filmowanie.Extensions.Initialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.SetStartupMode();

await builder.SetupConfigurationAsync();
builder.ConfigureLogging();
builder.Services.AddSignalR();

EnvironmentDependent.Invoke(new ()
{
    [StartupMode.LocalWithDevFrontend] = () => builder.Services.AddCors(o => o.AddPolicy("ViteLocalDevServer", p => p.WithOrigins(builder.Configuration["FrontendDevServer"]!)))
});

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

await EnvironmentDependent.InvokeAsync(new()
{
    [StartupMode.Production] = () => builder.SetupKeyVaultAsync()
});

builder.Services.AddMemoryCache();
builder.Services.RegisterPolicies();
builder.Services.RegisterCustomServices(builder.Configuration);
await builder.Services.RegisterDatabaseServicesAsync(builder.Configuration);
EnvironmentDependent.Invoke(new()
{
    [StartupMode.Local] = builder.Services.PersistDataProtectionKeysLocal,
    [StartupMode.Production] = () =>
    {
        builder.Services.PersistDataProtectionKeysBlob(builder.Configuration);
        builder.Services.AddOpenTelemetry().UseAzureMonitor();
    }
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureMassTransit(builder.Configuration);

var app = builder.Build();
var log = app.Services.GetRequiredService<ILogger<Program>>();
log.LogInformation($"Starting the app in mode: {Environment.Mode}...");

EnvironmentDependent.Invoke(new ()
{
    [StartupMode.LocalWithDevFrontend] = () => app.UseCors("ViteLocalDevServer"),
    [StartupMode.CopiledFrontend] = () => app.UseStaticFiles() 
});

app.ConfigureEndpoints();

await app.RunAsync();
