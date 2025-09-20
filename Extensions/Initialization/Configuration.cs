using Filmowanie.Abstractions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Filmowanie.Extensions.Initialization;

public static class Configuration
{
    public static void SetupConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddEnvironmentVariables();
        builder.Services.Configure<PushNotificationOptions>(builder.Configuration.GetSection("Vapid"));
        builder.Services.Configure<GoogleAuthOptions>(builder.Configuration.GetSection("GoogleOAuth"));
        builder.Services.Configure<CosmosOptions>(builder.Configuration);
    }
}