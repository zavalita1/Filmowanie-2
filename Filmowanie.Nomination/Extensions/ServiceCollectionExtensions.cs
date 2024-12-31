using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Nomination.Consts;
using Filmowanie.Nomination.Handlers;
using Filmowanie.Nomination.Helpers;
using Filmowanie.Nomination.Interfaces;
using Filmowanie.Nomination.Routes;
using Filmowanie.Nomination.Validators;
using Filmowanie.Nomination.Visitors;
using Microsoft.Extensions.DependencyInjection;

namespace Filmowanie.Nomination.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterNominationDomain(this IServiceCollection services)
    {
        services.AddScoped<INominationRoutes, NominationRoutes>();
        services.AddScoped<IGetNominationsVisitor, NominationsVisitor>();
        services.AddScoped<IGetNominationsDTOVisitor, NominationsVisitor>();
        services.AddScoped<INominationsCompleterVisitor, NominationsCommandVisitor>();
        services.AddScoped<INominationsResetterVisitor, NominationsCommandVisitor>();
        services.AddScoped<IMovieThatCanBeNominatedAgainEnricherVisitor, MovieThatCanBeNominatedAgainEnricherVisitor>();
        services.AddScoped<IGetPostersVisitor, GetPostersVisitor>();
        
        services.AddScoped<IFluentValidatorAdapter, NominationValidator>();
        services.AddScoped<IFluentValidatorAdapter, NominationMovieUrlValidator>();
        services.AddScoped<IFluentValidatorAdapter, NominationMovieIdValidator>();

        services.AddHttpClient(HttpClientNames.Filmweb, client =>
        {
            client.BaseAddress = new Uri(Urls.FilmwebUrl);
            // TODO poly retry?
        });

        services.AddSingleton<IFilmwebPathResolver, FilmwebPathResolver>();
        services.AddSingleton<IFilmwebPostersUrlsRetriever, FilmwebPostersUrlsRetriever>();
        services.AddSingleton<IFilmwebHandler, FilmwebHandler>();
        services.AddSingleton<IRoutesResultHelper, RoutesResultHelper>();

        return services;
    }
}