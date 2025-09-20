using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Account.Constants;
using Filmowanie.Account.Helpers;
using Filmowanie.Account.Interfaces;
using Filmowanie.Account.Mappers;
using Filmowanie.Account.Routes;
using Filmowanie.Account.Services;
using Filmowanie.Account.Validators;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

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
        services.AddScoped<IFluentValidatorAdapter, GoogleOAuthClientDTOValidator>();

        services.AddScoped<IAccountUserService, AccountUserService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddScoped<IAuthenticationManager, AuthenticationManager>();
        services.AddScoped<ICurrentUserAccessor, AuthenticationManager>();
        services.AddScoped<ISignUpService, AccountSignUpService>();

        services.AddSingleton<IDomainUserMapper, DomainUserMapper>();
        services.AddSingleton<IUserDtoMapper, UserDtoMapper>();

        services.AddSingleton<IHashHelper, HashHelper>();
        services.AddSingleton<IUserIdProvider, UserIdProvider>();
        services.AddSingleton<ILoginResultDataExtractor, LoginResultDataExtractor>();
        services.AddSingleton<ILoginResultDataExtractorDecorator, TokenLoginResultDataExtractorDecorator>();
        services.AddSingleton<ILoginDataExtractorAdapterFactory, LoginDataExtractorFactory>();
        services.AddSingleton<IRoutesResultHelper, RoutesResultHelper>();

        services.AddScoped<IHttpContextWrapper, HttpContextWrapper>();

        var retryPolicy = HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(3, x => TimeSpan.FromMilliseconds(10 * Math.Pow(2, x)));

        services.AddHttpClient(HttpClientNames.Google).AddPolicyHandler(retryPolicy);

        return services;
    }
}