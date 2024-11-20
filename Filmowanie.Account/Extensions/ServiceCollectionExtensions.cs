using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Account.Helpers;
using Filmowanie.Account.Interfaces;
using Filmowanie.Account.Repositories;
using Filmowanie.Account.Routes;
using Filmowanie.Account.Validators;
using Filmowanie.Account.Visitors;
using Microsoft.Extensions.DependencyInjection;

namespace Filmowanie.Account.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterAccountDomain(this IServiceCollection services)
    {
        services.AddScoped<IAccountRoutes, AccountRoutes>();

        services.AddScoped<IFluentValidatorAdapter, LoginCodeValidator>();
        services.AddScoped<IFluentValidatorAdapter, BasicAuthValidator>();
        services.AddScoped<IFluentValidatorAdapter, BasicAuthSignupValidator>();

        services.AddScoped<ICodeLoginVisitor, AccountVisitor>();
        services.AddScoped<ISignUpVisitor, AccountVisitor>();
        services.AddScoped<IBasicAuthLoginVisitor, AccountVisitor>();
        services.AddScoped<IUserIdentityVisitor, UserIdentityVisitor>();

        services.AddScoped<IUsersQueryRepository, UsersQueryRepository>();
        services.AddScoped<IUsersCommandRepository, UsersCommandRepository>();

        services.AddSingleton<IUserMapperVisitor, UserMapperVisitor>();

        services.AddSingleton<IHashHelper, HashHelper>();

        return services;
    }
}