using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Filmowanie.Abstractions.Configuration;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Database.Extensions;
using Filmowanie.Database.OptionsProviders;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.CosmosDb;

namespace Filmowanie.Extensions.Initialization;

internal static class Configuration
{
    public static async Task SetupConfigurationAsync(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddEnvironmentVariables();
        builder.Services.Configure<PushNotificationOptions>(builder.Configuration.GetSection("Vapid"));
        builder.Services.Configure<GoogleAuthOptions>(builder.Configuration.GetSection("GoogleOAuth"));
        builder.Services.Configure<CosmosOptions>(builder.Configuration);
        builder.Services.Configure<OpenAIOptions>(builder.Configuration.GetSection("OpenAI"));
        builder.Services.Configure<ImdbOptions>(builder.Configuration.GetSection("Imdb"));
        builder.Services.Configure<FilmwebOptions>(builder.Configuration.GetSection("Filmweb"));

        await EnvironmentDependent.InvokeAsync(new()
        {
            [StartupMode.WithCosmosEmulator] = () => SetupCosmosDbEmulator(builder)
        });
    }

    private static async Task SetupCosmosDbEmulator(WebApplicationBuilder builder)
    {
        var cosmosDbContainer = new CosmosDbBuilder()
       .WithImage("mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest")
       .WithReuse(true)
       .Build();

        await cosmosDbContainer.StartAsync();

        var cstr = cosmosDbContainer.GetConnectionString();
        var containerPort = cosmosDbContainer.GetMappedPublicPort();
        CosmosDbContainerInstance.Instance = cosmosDbContainer;
        Console.WriteLine($"Started cosmos db emulator container. Port: {containerPort}.");
        var certUrl = $"https://localhost:{containerPort}/_explorer/emulator.pem";
        var httpHandler = new HttpClientHandler();
        httpHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
        httpHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        using var httpClient = new HttpClient(httpHandler);
        using var certRequest = new HttpRequestMessage(HttpMethod.Get, certUrl);

        using var response = await httpClient.SendAsync(certRequest);
        var certText = await response.Content.ReadAsStringAsync();
        var certBytes = Encoding.UTF8.GetBytes(certText);
        var cert = new X509Certificate2(certBytes);
        using var certStore = new X509Store(StoreName.TrustedPeople, StoreLocation.LocalMachine);
        certStore.Open(OpenFlags.ReadWrite);
        certStore.Add(cert); // FYI this created new entry in cert store, it might get cluttered if you run this a lot.

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>()
        {
            [CosmosOptions.ConnectionStringConfigKeyName] = cstr
        });

        builder.Services.ConfigureCosmosClientForEmulatedDb(cosmosDbContainer.HttpClient);
    }
}