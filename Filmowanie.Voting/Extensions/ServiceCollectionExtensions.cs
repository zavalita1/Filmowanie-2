using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Voting.Deciders;
using Filmowanie.Voting.Deciders.PickUserNomination;
using Filmowanie.Voting.Interfaces;
using Filmowanie.Voting.Mappers;
using Filmowanie.Voting.Retrievers;
using Filmowanie.Voting.Routes;
using Filmowanie.Voting.Services;
using Filmowanie.Voting.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Filmowanie.Voting.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterVotingDomain(this IServiceCollection services)
    {
        services.AddScoped<IFluentValidatorAdapter, VoteValidator>();
        services.AddScoped<IFluentValidatorAdapter, VotingSessionIdValidator>();

        services.AddScoped<IVotingSessionRoutes, VotingSessionRoutes>();
        services.AddScoped<IAdminVotingSessionRoutes, AdminVotingSessionRoutes>();
        services.AddScoped<IVotingResultRoutes, VotingResultRoutes>();
        
        services.AddScoped<IMovieVotingSessionService, MovieVotingSessionService>();

        services.AddScoped<IMoviesForVotingSessionEnricher, MoviesForVotingSessionEnricher>();

        services.AddScoped<IVotingSessionService, VotingSessionService>();
        services.AddScoped<ICurrentVotingSessionIdAccessor, VotingSessionService>();
        services.AddSingleton<IVotingSessionMapper, VotingSessionMapper>();

        services.AddScoped<IVotingStateManager, VotingSessionStateManager>();

        services.AddScoped<IVoteService, VoteService>();
        services.AddScoped<IVotingResultsRetriever, VotingResultsRetriever>();
        services.AddScoped<INominationsRetriever, NominationsRetriever>();

        services.AddSingleton<IVotingDeciderFactory, VotingDeciderFactory>();
        services.AddSingleton<IPickUserToNominateStrategyFactory, PickUserToNominateStrategyFactory>();
        services.AddSingleton<IPickUserToNominateContextRetriever, PickUserToNominateContextRetriever>();

        return services;
    }
}