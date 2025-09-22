using Filmowanie.Abstractions.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Filmowanie.Database.Extensions;

public interface ICosmosClientOptionsDecorator
{
    public CosmosClientOptions ClientOptions { get; }

    public CosmosOptions CosmosOptions { get; }

    public void Apply(CosmosDbContextOptionsBuilder optionsBuilder);
   
}