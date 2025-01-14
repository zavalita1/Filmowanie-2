using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Account.Helpers;
using Filmowanie.Account.Interfaces;
using Filmowanie.Account.Routes;
using Filmowanie.Account.Validators;
using Filmowanie.Account.Visitors;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Filmowanie.Account.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterAccountDomain(this IServiceCollection services)
    {
        services.AddScoped<IAccountRoutes, AccountRoutes>();
        services.AddScoped<IAccountsAministrationRoutes, AccountsAdministrationRoutes>();

        services.AddScoped<IFluentValidatorAdapter, LoginCodeValidator>();
        services.AddScoped<IFluentValidatorAdapter, BasicAuthValidator>();
        services.AddScoped<IFluentValidatorAdapter, BasicAuthSignupValidator>();
        services.AddScoped<IFluentValidatorAdapter, UserDTOValidator>();
        services.AddScoped<IFluentValidatorAdapter, UserIdValidator>();

        services.AddScoped<ICodeLoginVisitor, AccountCodeLoginVisitor>();
        services.AddScoped<ISignUpVisitor, AccountSignUpVisitor>();
        services.AddScoped<IBasicAuthLoginVisitor, AccountBasicAuthLoginVisitor>();
        services.AddScoped<IUserIdentityVisitor, UserIdentityVisitor>();

        services.AddSingleton<IUserMapperVisitor, UserMapperVisitor>();
        services.AddScoped<IEnrichUserVisitor, EnrichUserVisitor>();
        services.AddScoped<IUserReverseMapperVisitor, UserReverseMapperVisitor>();

        services.AddScoped<IGetAllUsersVisitor, GetAllUsersVisitor>();
        services.AddScoped<IAddUserVisitor, AddUserVisitor>();

        services.AddSingleton<IHashHelper, HashHelper>();
        services.AddSingleton<IUserIdProvider, UserIdProvider>();
        services.AddSingleton<ILoginResultDataExtractor, LoginResultDataExtractor>();

        services.AddScoped<IHttpContextWrapper, HttpContextWrapper>();

        return services;
    }
}