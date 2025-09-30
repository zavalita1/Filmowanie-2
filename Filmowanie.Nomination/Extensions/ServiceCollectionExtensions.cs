using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Nomination.Consts;
using Filmowanie.Nomination.Helpers;
using Filmowanie.Nomination.Interfaces;
using Filmowanie.Nomination.Mappers;
using Filmowanie.Nomination.Routes;
using Filmowanie.Nomination.Services;
using Filmowanie.Nomination.Validators;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using FilmwebHandler = Filmowanie.Nomination.Services.FilmwebHandler;

namespace Filmowanie.Nomination.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterNominationDomain(this IServiceCollection services)
    {
        services.AddScoped<INominationRoutes, NominationRoutes>();

        services.AddScoped<INominationsService, NominationsService>();
        services.AddScoped<INominationsEnricher, NominationsEnricher>();
        services.AddSingleton<INominationsMapper, NominationsMapper>();

        services.AddScoped<IMoviePostersService, MoviePostersService>();
        
        services.AddScoped<IFluentValidatorAdapter, NominationDtoValidator>();
        services.AddScoped<IFluentValidatorAdapter, NominationMovieValidator>();
        services.AddScoped<IFluentValidatorAdapter, NominationMovieUrlValidator>();
        services.AddScoped<IFluentValidatorAdapter, NominationMovieIdValidator>();

        var retryPolicy = HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(3, x => TimeSpan.FromMilliseconds(100 * Math.Pow(2, x)));

        services.AddHttpClient(HttpClientNames.Filmweb).AddPolicyHandler(retryPolicy);

        services.AddSingleton<IFilmwebPathResolver, FilmwebPathResolver>();
        services.AddSingleton<IFilmwebPostersUrlsRetriever, FilmwebPostersUrlsRetriever>();
        services.AddScoped<IFilmwebHandler, FilmwebHandler>();
        services.AddSingleton<IRoutesResultHelper, RoutesResultHelper>();

        return services;
    }
}