using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Filmowanie.Account.Constants;
using Filmowanie.Account.Extensions;
using Filmowanie.Database.Contants;
using Filmowanie.Database.Entities.Events;
using Filmowanie.Database.Extensions;
using Filmowanie.Extensions;
using Filmowanie.Filters;
using Filmowanie.Voting.Extensions;
using Filmowanie.Voting.Sagas;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLogger;
using Environment = Filmowanie.Abstractions.Enums.Environment;

var builder = WebApplication
    .CreateBuilder(args);

var environment = builder.Environment.IsDevelopment() ? Environment.Development : Environment.Production;

builder.Logging.ClearProviders();
builder.Logging.AddZLoggerConsole();
var currentDll = Assembly.GetExecutingAssembly().Location;
var currentDir = Path.GetDirectoryName(currentDll);
var logPath = $"{currentDir}\\AppLog.txt";
builder.Logging.AddZLoggerFile(logPath);

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
    await SetupKeyVaultAsync(builder);
}

builder.Services.RegisterPolicies();
builder.Services.RegisterCustomServices(builder.Configuration, environment);
builder.Services.RegisterDatabaseServices(builder.Configuration, environment);

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    var dbConnectionString = builder.Configuration["dbConnectionString"]!;
    x.SetCosmosSagaRepositoryProvider(dbConnectionString, cosmosConfig =>
    {
        cosmosConfig.DatabaseId = "db-filmowanie2";
        cosmosConfig.CollectionId = DbContainerNames.Events;
    });

    var entryAssembly = new [] {Assembly.GetEntryAssembly()!, typeof(VotingStateInstance).Assembly}; // TODO

    x.AddConsumers(entryAssembly);
  //  x.AddSaga<VotingStateInstance>();
    x.AddSagaStateMachine<VotingStateMachine, VotingStateInstance>();
    x.AddActivities(entryAssembly);

    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

// TODO database integration
var app = builder.Build();
if (environment != Environment.Development)
{
    app.UseSpaStaticFiles();
}

var apiGroup = app.MapGroup("api");
apiGroup.AddEndpointFilter<LoggingActionFilter>();
apiGroup.RegisterAccountRoutes();
apiGroup.RegisterVotingRoutes();

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

// TODO configure signalr hubs
app.Run();
return;

async Task SetupKeyVaultAsync(WebApplicationBuilder webApplicationBuilder)
{
    var keyVaultName = webApplicationBuilder.Configuration["KeyVaultName"];
    var kvUri = $"https://{keyVaultName}.vault.azure.net";

    var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
    var keyVaultConfigurationProvider = new ConcurrentDictionary<string, string>();
    var keys = client.GetPropertiesOfSecrets().AsPages().ToArray().SelectMany(x => x.Values);
    await Parallel.ForEachAsync(keys, async (k, cancel) =>
    {
        var secret = await client.GetSecretAsync(k.Name, cancellationToken: cancel);
        keyVaultConfigurationProvider.AddOrUpdate(k.Name, secret.Value.Value, (_, _) => throw new NotSupportedException("Secret names must be unique!"));
    });
    webApplicationBuilder.Configuration.AddInMemoryCollection(keyVaultConfigurationProvider);
}