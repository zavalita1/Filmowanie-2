using System;
using System.Reflection;
using Filmowanie.Database.Contants;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Notification.Consumers;
using Filmowanie.Voting.Consumers;
using Filmowanie.Voting.Sagas;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Filmowanie.Extensions.Initialization;

internal static class MassTransitInit
{
    public static void ConfigureMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConfigureEndpointsCallback((context, name, cfg) =>
            {
                cfg.UseMessageRetry(r => r.Incremental(5, TimeSpan.Zero, TimeSpan.FromMilliseconds(250)));
            });

            x.SetKebabCaseEndpointNameFormatter();
            var dbConnectionString = configuration["dbConnectionString"]!; // TODO cosmos options 
            x.SetCosmosSagaRepositoryProvider(dbConnectionString, cosmosConfig =>
            {
                cosmosConfig.DatabaseId = "db-filmowanie2";
                cosmosConfig.CollectionId = DbContainerNames.Events;
            });

            var entryAssembly = new[]
            {
                Assembly.GetEntryAssembly()!,
                typeof(VotingStateInstance).Assembly,
                typeof(Nomination.Consumers.ResultsConfirmedConsumer).Assembly,
                typeof(ConcludeVotingEventConsumer).Assembly,
                typeof(ResultsConfirmedConsumer).Assembly,
            }; // TODO

            x.AddConsumers(entryAssembly);
            x.AddSagaStateMachine<VotingStateMachine, VotingStateInstance>();
            x.AddActivities(entryAssembly);

            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });
    }
}