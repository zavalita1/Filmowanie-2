using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Notification.Interfaces;
using Filmowanie.Notification.Routes;
using Filmowanie.Notification.Services;
using Filmowanie.Notification.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Filmowanie.Notification.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterNotificationDomain(this IServiceCollection services)
    {
        services.AddScoped<IPushNotificationRoutes, PushNotificationRoutes>();
        services.AddScoped<IFluentValidatorAdapter, PushSubscriptionDTOValidator>();
        services.AddScoped<IPushNotificationService, PushNotificationService>();

        services.AddSingleton<IRoutesResultHelper, RoutesResultHelper>();

        return services;
    }
}