using Filmowanie.Database.Extensions;
using MassTransit;
using Microsoft.Azure.Cosmos;

namespace Filmowanie.Infrastructure;

internal sealed class MassTransitClientFactory : ICosmosClientFactory
{
    private static CosmosClient? CosmosClient { get; set; }

    private static readonly object Locker = new();

    private readonly ICosmosClientOptionsProvider cosmosClientOptionsProvider;

    public MassTransitClientFactory(ICosmosClientOptionsProvider cosmosClientOptionsProvider)
    {
        this.cosmosClientOptionsProvider = cosmosClientOptionsProvider;
    }

    public CosmosClient GetCosmosClient<T>(string clientName) where T : class, ISaga
    {
        if (CosmosClient != null)
            return CosmosClient;

        lock (Locker)
        {
            if (CosmosClient != null) return CosmosClient;

            var options = this.cosmosClientOptionsProvider.Get();
            CosmosClient = new CosmosClient(options.CosmosOptions.ConnectionString, options.ClientOptions);
            return CosmosClient;
        }
    }
}
