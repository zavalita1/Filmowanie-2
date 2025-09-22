using Testcontainers.CosmosDb;

namespace Filmowanie.Database.OptionsProviders;

public static class CosmosDbContainerInstance
{
    /// <summary>
    /// Only for local setup with cosmos db emulator.
    /// </summary>
    public static CosmosDbContainer? Instance { get; set; }
}