using Microsoft.Extensions.Configuration;

namespace Filmowanie.Abstractions.Configuration;

public sealed class CosmosOptions
{
    public const string ConnectionStringConfigKeyName = "dbConnectionString";
    public const string DbNameKeyName = "dbName";
    public const string ExtensiveEFLoggingEnabledKeyName = nameof(ExtensiveEFLoggingEnabled);

    [ConfigurationKeyName(ConnectionStringConfigKeyName)]
    public required string ConnectionString { get; set; }

    public required bool ExtensiveEFLoggingEnabled { get; set; }

    [ConfigurationKeyName(DbNameKeyName)]
    public required string DbName { get; set; }
}