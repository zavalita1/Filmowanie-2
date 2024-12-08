using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Filmowanie.Abstractions.Constants;
using Filmowanie.Account.Constants;
using Filmowanie.Account.Extensions;
using Filmowanie.Database.Contants;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Extensions;
using Filmowanie.Extensions;
using Filmowanie.Filters;
using Filmowanie.Nomination.Extensions;
using Filmowanie.Voting.Consumers;
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

var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment.IsDevelopment() ? Environment.Development : Environment.Production;

ConfigureLogging(builder);

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

ConfigureMassTransit(builder);

var app = builder.Build();
if (environment != Environment.Development)
{
    app.UseSpaStaticFiles();
}

ConfigureEndpoints(app, environment);

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

void ConfigureEndpoints(WebApplication webApplication, Environment appEnvironment)
{
    var apiGroup = webApplication.MapGroup("api");
    apiGroup.AddEndpointFilter<LoggingActionFilter>();
    apiGroup.RegisterAccountRoutes();
    apiGroup.RegisterVotingRoutes();
    apiGroup.RegisterNominationRoutes();

    webApplication.UseWhen(
        context => !context.Request.Path.StartsWithSegments("/api"),
        then => then.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (appEnvironment != Environment.Production)
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            }
        ));
}

void ConfigureLogging(WebApplicationBuilder appBuilder)
{
    appBuilder.Logging.ClearProviders();
    appBuilder.Logging.AddZLoggerConsole();
    var currentDll = Assembly.GetExecutingAssembly().Location;
    var currentDir = Path.GetDirectoryName(currentDll);
    var logPath = $"{currentDir}\\AppLog.txt";
    appBuilder.Logging.AddZLoggerFile(logPath);
}

void ConfigureMassTransit(WebApplicationBuilder appBuilder)
{
    appBuilder.Services.AddMassTransit(x =>
    {
        x.AddConfigureEndpointsCallback((context, name, cfg) =>
        {
            cfg.UseMessageRetry(r => r.Immediate(5));
        });

        x.SetKebabCaseEndpointNameFormatter();
        var dbConnectionString = appBuilder.Configuration["dbConnectionString"]!;
        x.SetCosmosSagaRepositoryProvider(dbConnectionString, cosmosConfig =>
        {
            cosmosConfig.DatabaseId = "db-filmowanie2";
            cosmosConfig.CollectionId = DbContainerNames.Events;
        });

        var entryAssembly = new []
        {
            Assembly.GetEntryAssembly()!, 
            typeof(VotingStateInstance).Assembly, 
            typeof(Filmowanie.Nomination.Consumers.VotingConcludedConsumer).Assembly
        }; // TODO

        x.AddConsumer<VotingConcludedConsumer>();
        x.AddConsumer<Filmowanie.Nomination.Consumers.VotingConcludedConsumer>();
        x.AddSagaStateMachine<VotingStateMachine, VotingStateInstance>();
        x.AddActivities(entryAssembly);

        x.UsingInMemory((context, cfg) =>
        {
            cfg.ConfigureEndpoints(context);
        });
    });
}