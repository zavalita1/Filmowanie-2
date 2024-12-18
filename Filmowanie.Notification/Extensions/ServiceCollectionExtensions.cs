using Filmowanie.Notification.Interfaces;
using Filmowanie.Notification.Routes;
using Microsoft.Extensions.DependencyInjection;

namespace Filmowanie.Notification.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterNotificationDomain(this IServiceCollection services)
    {
        services.AddScoped<IPushNotificationRoutes, PushNotificationRoutes>();

        return services;
    }
}