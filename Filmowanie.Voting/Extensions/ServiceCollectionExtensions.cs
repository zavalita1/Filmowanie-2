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

        services.AddScoped<IVotingSessionRoutes, CurrentVotingRoutes>();
        services.AddScoped<IAdminVotingSessionRoutes, AdminVotingSessionRoutes>();
        services.AddScoped<IVotingResultRoutes, VotingResultRoutes>();
        
        services.AddScoped<IMovieVotingResultService, MovieVotingResultService>();
        services.AddScoped<ICurrentVotingService, CurrentVotingService>();

        services.AddScoped<IMoviesForVotingSessionEnricher, MoviesForVotingSessionEnricher>();

        services.AddScoped<IVotingSessionService, VotingSessionService>();
        services.AddScoped<ICurrentVotingSessionIdAccessor, VotingSessionService>();
        services.AddScoped<IVotingMappersComposite, VotingMappersComposite>();
        services.AddScoped<IVotingSessionIdMapper, VotingSessionIdMapper>();
        services.AddSingleton<IVotingStateMapper, VotingStateMapper>();
        services.AddSingleton<IHistoryDtoMapper, HistoryDtoMapper>();
        services.AddSingleton<IMovieDtoMapper, MovieDtoMapper>();
        services.AddSingleton<IVotingSessionsDTOMapper, VotingSessionsDTOMapper>();

        services.AddScoped<IVotingStateManager, VotingSessionStateManager>();

        services.AddScoped<IVoteService, VoteService>();
        services.AddScoped<IVotingResultsRetriever, VotingResultsRetriever>();
        services.AddScoped<INominationsRetriever, NominationsRetriever>();
        services.AddScoped<ICurrentVotingStatusRetriever, CurrentVotingStatusRetriever>();

        services.AddSingleton<IVotingDeciderFactory, VotingDeciderFactory>();
        services.AddSingleton<IPickUserToNominateStrategyFactory, PickUserToNominateStrategyFactory>();
        services.AddSingleton<IPickUserToNominateContextRetriever, PickUserToNominateContextRetriever>();

        return services;
    }
}