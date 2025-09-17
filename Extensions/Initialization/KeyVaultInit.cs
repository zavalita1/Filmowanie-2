using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Filmowanie.Extensions.Initialization;

internal static class KeyVaultInit
{
    public static async Task SetupKeyVaultAsync(this WebApplicationBuilder webApplicationBuilder)
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
        webApplicationBuilder.Configuration.AddInMemoryCollection(keyVaultConfigurationProvider!);
    }
}