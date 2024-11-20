using Filmowanie.Account.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Filmowanie.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterPolicies(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddAuthorizationBuilder()
            .AddPolicy(Schemes.Admin, policy =>
                policy
                    .AddAuthenticationSchemes(Schemes.Cookie)
                    .RequireClaim(ClaimsTypes.IsAdmin, "true"));

        serviceCollection.AddAuthorizationBuilder()
            .AddPolicy(Schemes.Cookie, policy =>
                policy
                    .AddAuthenticationSchemes(Schemes.Cookie)
                    .RequireClaim(ClaimsTypes.IsAdmin, "true"));

        return serviceCollection;
    }
}