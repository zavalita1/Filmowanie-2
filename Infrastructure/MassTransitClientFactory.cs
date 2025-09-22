using DotNet.Testcontainers.Containers;
using Filmowanie.Database.Extensions;
using MassTransit;
using Microsoft.Azure.Cosmos;
using System;
using Testcontainers.CosmosDb;

namespace Filmowanie;

internal sealed class MassTransitClientFactory : ICosmosClientFactory
{
    private static CosmosClient? CosmosClient { get; set; }

    private readonly static object locker = new();

    private readonly ICosmosClientOptionsProvider cosmosClientOptionsProvider;

    public MassTransitClientFactory(ICosmosClientOptionsProvider cosmosClientOptionsProvider)
    {
        this.cosmosClientOptionsProvider = cosmosClientOptionsProvider;
    }

    public CosmosClient GetCosmosClient<T>(string clientName) where T : class, ISaga
    {
        try
        {
            Console.WriteLine("CALLED WITH : " + clientName);
            if (CosmosClient != null)
                return CosmosClient;

            lock (locker)
            {
                Console.WriteLine("CALLED2 WITH : " + clientName);
                if (CosmosClient != null) return CosmosClient;

                Console.WriteLine("CALLED3 WITH : " + clientName);
                var options = this.cosmosClientOptionsProvider.Get();
                //options.ClientOptions.HttpClientFactory = null;
                CosmosClient = new CosmosClient(options.CosmosOptions.ConnectionString, options.ClientOptions);
                Console.WriteLine("CALLED4 WITH : " + clientName);
                return CosmosClient;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("looo 5");
            Console.WriteLine(e);
            throw;
        }
    }
}
