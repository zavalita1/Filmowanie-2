using Filmowanie.Abstractions.Configuration;
using Filmowanie.Database.Extensions;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;

namespace Filmowanie.Database.OptionsProviders;

internal sealed class EmulatedCosmosClientOptionsProvider : ICosmosClientOptionsProvider
{
    private readonly HttpClient client;
    private readonly CosmosOptions options;

    internal EmulatedCosmosClientOptionsProvider(HttpClient client, IOptions<CosmosOptions> options)
    {
        this.client = client;
        this.options = options.Value;
    }

    public ICosmosClientOptionsDecorator Get()
    {
        var clientOptions = new CosmosClientOptions
        {
            ConnectionMode = ConnectionMode.Gateway,
            HttpClientFactory = () =>
            {
                var handler = typeof(HttpMessageInvoker).GetField("_handler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(this.client);
                var typedHandler = (HttpMessageHandler)handler!;
                var result = new HttpClient(typedHandler); // tcp port exhaustion is not a concern for local env.
                return result;
            }
        };
        return new CosmosClientOptionsDecorator(clientOptions, this.options);
    }

    private sealed class CosmosClientOptionsDecorator : ICosmosClientOptionsDecorator
    {
        public CosmosClientOptions ClientOptions { get; }

        public CosmosOptions CosmosOptions { get; }

        public CosmosClientOptionsDecorator(CosmosClientOptions clientOptions, CosmosOptions options)
        {
            ClientOptions = clientOptions;
            CosmosOptions = options;
        }

        public void Apply(CosmosDbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConnectionMode(ClientOptions.ConnectionMode);
            optionsBuilder.HttpClientFactory(ClientOptions.HttpClientFactory);
        }
    }
}
