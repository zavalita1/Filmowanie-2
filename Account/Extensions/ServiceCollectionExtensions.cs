using Filmowanie.Account.Mappers;
using Filmowanie.Account.Repositories;
using Filmowanie.Account.Services;
using Filmowanie.Account.Validators;
using Filmowanie.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Filmowanie.Account.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterAccountDomain(this IServiceCollection services)
    {
        services.AddScoped<IFluentValidatorAdapter, LoginCodeValidator>();
        services.AddScoped<IFluentValidatorAdapter, BasicAuthValidator>();
        services.AddScoped<IFluentValidatorAdapter, BasicAuthSignupValidator>();

        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IUserIdentityService, UserIdentityService>();

        services.AddScoped<IUsersQueryRepository, UsersQueryRepository>();
        services.AddScoped<IUsersCommandRepository, UsersCommandRepository>();

        services.AddSingleton<IUserMapper, UserMapper>();

        return services;
    }
}