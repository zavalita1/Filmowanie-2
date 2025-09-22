using Filmowanie.Abstractions.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;

namespace Filmowanie.Database.Extensions;

internal sealed class DefaultCosmosClientOptionsProvider : ICosmosClientOptionsProvider
{
    private readonly IOptions<CosmosOptions> options;

    public DefaultCosmosClientOptionsProvider(IOptions<CosmosOptions> options)
    {
        this.options = options;
    }

    public ICosmosClientOptionsDecorator Get() => new NoopOptionsDecorator(this.options.Value);

    private sealed class NoopOptionsDecorator : ICosmosClientOptionsDecorator
    {
        public NoopOptionsDecorator(CosmosOptions options)
        {
            CosmosOptions = options;
        }

        public CosmosClientOptions ClientOptions => new();

        public CosmosOptions CosmosOptions { get; }

        public void Apply(CosmosDbContextOptionsBuilder optionsBuilder)
        { }
    }
}
