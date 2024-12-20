using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Providers;
using Filmowanie.Account.Extensions;
using Filmowanie.Infrastructure;
using Filmowanie.Interfaces;
using Filmowanie.Nomination.Extensions;
using Filmowanie.Voting.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Filmowanie.Extensions;

public static class RegisterServices
{
    public static void RegisterCustomServices(this IServiceCollection services, IConfiguration configuration, Environment environment)
    {
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<IFluentValidatorAdapterProvider, FluentValidatorAdapterProvider>();
        services.AddScoped<IFluentValidationAdapterFactory, FluentValidationAdapterFactory>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IGuidProvider, GuidProvider>();

        services.RegisterAccountDomain();
        services.RegisterVotingDomain();
        services.RegisterNominationDomain();
    }
}