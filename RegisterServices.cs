using Filmowanie.Account.Extensions;
using Filmowanie.Infrastructure;
using Filmowanie.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Filmowanie;

public static class RegisterServices
{
    public static void RegisterCustomServices(IServiceCollection services, IConfiguration configuration, Enums.Environment environment)
    {
        services.AddScoped<IFluentValidatorAdapterFactory, FluentValidatorAdapterFactory>();
        
        services.RegisterAccountDomain();
    }
}