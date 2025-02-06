using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Notification.Interfaces;
using Filmowanie.Notification.Routes;
using Filmowanie.Notification.Services;
using Filmowanie.Notification.Validators;
using Filmowanie.Notification.Visitors;
using Microsoft.Extensions.DependencyInjection;

namespace Filmowanie.Notification.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterNotificationDomain(this IServiceCollection services)
    {
        services.AddScoped<IPushNotificationRoutes, PushNotificationRoutes>();
        services.AddScoped<IFluentValidatorAdapter, PushSubscriptionDTOValidator>();
        services.AddScoped<ISavePushNotificationVisitor, SavePushNotificationVisitor>();
        services.AddScoped<INotifyAllPushSubscribersVisitor, NotifyAllPushSubscribersVisitor>();

        services.AddSingleton<IRoutesResultHelper, RoutesResultHelper>();

        return services;
    }
}