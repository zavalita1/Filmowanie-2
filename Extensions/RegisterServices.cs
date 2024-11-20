using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Account.Extensions;
using Filmowanie.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Filmowanie.Extensions;

public static class RegisterServices
{
    public static void RegisterCustomServices(this IServiceCollection services, IConfiguration configuration, Enums.Environment environment)
    {
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<IFluentValidatorAdapterFactory, FluentValidatorAdapterFactory>();
        
        services.RegisterAccountDomain();
    }
}