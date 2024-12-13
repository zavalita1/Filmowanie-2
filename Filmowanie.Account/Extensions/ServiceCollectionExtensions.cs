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

        services.AddScoped<ICodeLoginVisitor, AccountVisitor>();
        services.AddScoped<ISignUpVisitor, AccountVisitor>();
        services.AddScoped<IBasicAuthLoginVisitor, AccountVisitor>();
        services.AddScoped<IUserIdentityVisitor, UserIdentityVisitor>();

        services.AddScoped<IUserMapperVisitor, UserDTOMapperVisitor>();
        services.AddScoped<IEnrichUserVisitor, UserDTOMapperVisitor>();
        services.AddScoped<IUserReverseMapperVisitor, UserDTOMapperVisitor>();

        services.AddScoped<IGetAllUsersVisitor, UsersManagementVisitor>();
        services.AddScoped<IAddUserVisitor, UsersManagementVisitor>();

        services.AddSingleton<IHashHelper, HashHelper>();
        services.AddSingleton<IUserIdProvider, UserIdProvider>();

        return services;
    }
}