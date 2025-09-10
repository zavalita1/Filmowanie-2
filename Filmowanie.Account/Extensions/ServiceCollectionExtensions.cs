using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Account.Helpers;
using Filmowanie.Account.Interfaces;
using Filmowanie.Account.Mappers;
using Filmowanie.Account.Routes;
using Filmowanie.Account.Services;
using Filmowanie.Account.Validators;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Filmowanie.Account.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterAccountDomain(this IServiceCollection services)
    {
        services.AddScoped<IAccountRoutes, AccountRoutes>();
        services.AddScoped<IAccountsAdministrationRoutes, AccountsAdministrationRoutes>();

        services.AddScoped<IFluentValidatorAdapter, LoginCodeValidator>();
        services.AddScoped<IFluentValidatorAdapter, BasicAuthValidator>();
        services.AddScoped<IFluentValidatorAdapter, BasicAuthSignupValidator>();
        services.AddScoped<IFluentValidatorAdapter, UserDTOValidator>();
        services.AddScoped<IFluentValidatorAdapter, UserIdValidator>();

        services.AddScoped<IAccountUserService, AccountUserService>();
        services.AddScoped<IAuthenticationManager, AuthenticationManager>();
        services.AddScoped<ICurrentUserAccessor, AuthenticationManager>();
        services.AddScoped<ISignUpService, AccountSignUpService>();

        services.AddSingleton<IDomainUserMapper, DomainUserMapper>();
        services.AddSingleton<IUserDtoMapper, UserDtoMapper>();

        services.AddSingleton<IHashHelper, HashHelper>();
        services.AddSingleton<IUserIdProvider, UserIdProvider>();
        services.AddSingleton<ILoginResultDataExtractor, LoginResultDataExtractor>();
        services.AddSingleton<IRoutesResultHelper, RoutesResultHelper>();

        services.AddScoped<IHttpContextWrapper, HttpContextWrapper>();

        return services;
    }
}