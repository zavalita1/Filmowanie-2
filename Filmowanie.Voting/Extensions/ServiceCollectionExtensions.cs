using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Voting.Deciders;
using Filmowanie.Voting.Deciders.PickUserNomination;
using Filmowanie.Voting.Interfaces;
using Filmowanie.Voting.Retrievers;
using Filmowanie.Voting.Routes;
using Filmowanie.Voting.Validators;
using Filmowanie.Voting.Visitors;
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
        
        services.AddScoped<IWinnersMetadataMapperVisitor, WinnersMetadataMapperVisitor>();
        services.AddScoped<IHistoryDTOMapperVisitor, HistoryDTOMapperVisitor>();
        services.AddScoped<IHistoryStandingsDTOMapperVisitor, HistoryStandingsDTOMapper>();
        services.AddScoped<IGetMoviesForVotingSessionVisitor, GetMoviesForVotingSessionVisitor>();
        services.AddScoped<IEnrichMoviesForVotingSessionWithPlaceholdersVisitor, EnrichMoviesForVotingSessionWithPlaceholdersVisitor>();
        services.AddScoped<IGetVotingSessionsMetadataVisitor, VotingSessionResultVisitor>();
        services.AddScoped<IGetVotingResultDTOVisitor, GetVotingResultDTOVisitor>();

        services.AddScoped<IVotingSessionService, VotingSessionService>();
        services.AddScoped<ICurrentVotingSessionIdAccessor, VotingSessionService>();

        services.AddScoped<IRequireCurrentVotingSessionIdVisitor, VotingSessionIdQueryVisitor>();
        services.AddScoped<IGetCurrentVotingSessionIdVisitor, VotingSessionIdQueryVisitor>();
        services.AddScoped<IGetCurrentVotingSessionStatusVisitor, VotingSessionIdQueryVisitor>();
        services.AddScoped<IStartNewVotingVisitor, StartNewVotingVisitor>();
        services.AddScoped<IConcludeVotingVisitor, ConcludeVotingVisitor>();

        services.AddScoped<IVotingSessionStatusMapperVisitor, VotingMapperVisitor>();
        services.AddScoped<IVotingSessionIdMapperVisitor, VotingMapperVisitor>();
        services.AddScoped<IVotingSessionsMapperVisitor, VotingMapperVisitor>();
        services.AddScoped<IAknowledgedMapperVisitor, VotingMapperVisitor>();

        services.AddScoped<IVoteVisitor, VoteVisitor>();
        services.AddScoped<IVotingResultsRetriever, VotingResultsRetriever>();
        services.AddScoped<INominationsRetriever, NominationsRetriever>();

        services.AddSingleton<IVotingDeciderFactory, VotingDeciderFactory>();
        services.AddSingleton<IPickUserToNominateStrategyFactory, PickUserToNominateStrategyFactory>();
        services.AddSingleton<IPickUserToNominateContextRetriever, PickUserToNominateContextRetriever>();

        return services;
    }
}