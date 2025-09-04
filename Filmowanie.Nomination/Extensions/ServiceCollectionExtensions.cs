using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Nomination.Consts;
using Filmowanie.Nomination.Handlers;
using Filmowanie.Nomination.Helpers;
using Filmowanie.Nomination.Interfaces;
using Filmowanie.Nomination.Routes;
using Filmowanie.Nomination.Services;
using Filmowanie.Nomination.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Filmowanie.Nomination.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterNominationDomain(this IServiceCollection services)
    {
        services.AddScoped<INominationRoutes, NominationRoutes>();

        services.AddScoped<INominationsService, NominationsService>();

        services.AddScoped<IMoviePostersService, MoviePostersService>();
        
        services.AddScoped<IFluentValidatorAdapter, NominationValidator>();
        services.AddScoped<IFluentValidatorAdapter, NominationMovieUrlValidator>();
        services.AddScoped<IFluentValidatorAdapter, NominationMovieIdValidator>();

        services.AddHttpClient(HttpClientNames.Filmweb, client =>
        {
            client.BaseAddress = new Uri(Urls.FilmwebUrl);
            // TODO polly retry?
        });

        services.AddSingleton<IFilmwebPathResolver, FilmwebPathResolver>();
        services.AddSingleton<IFilmwebPostersUrlsRetriever, FilmwebPostersUrlsRetriever>();
        services.AddSingleton<IFilmwebHandler, FilmwebHandler>();
        services.AddSingleton<IRoutesResultHelper, RoutesResultHelper>();

        return services;
    }
}