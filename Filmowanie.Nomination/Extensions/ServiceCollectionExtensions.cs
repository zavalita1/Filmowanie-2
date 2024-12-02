using Filmowanie.Nomination.Interfaces;
using Filmowanie.Nomination.Routes;
using Filmowanie.Nomination.Visitors;
using Microsoft.Extensions.DependencyInjection;

namespace Filmowanie.Nomination.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterNominationDomain(this IServiceCollection services)
    {
        services.AddScoped<INominationRoutes, NominationRoutes>();
        services.AddScoped<IGetNominationsVisitor, NominationsVisitor>();

        return services;
    }
}