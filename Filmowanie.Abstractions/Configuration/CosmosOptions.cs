using Microsoft.Extensions.Configuration;

namespace Filmowanie.Abstractions.Configuration;

public sealed class CosmosOptions
{
    [ConfigurationKeyName("dbConnectionString")]
    public required string ConnectionString { get; set; }
}