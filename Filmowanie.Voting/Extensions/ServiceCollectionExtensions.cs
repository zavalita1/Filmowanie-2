using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Voting.Deciders;
using Filmowanie.Voting.Deciders.PickUserNomination;
using Filmowanie.Voting.Interfaces;
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

        services.AddScoped<IGetMoviesForVotingSessionVisitor, MoviesVisitor>();
        services.AddScoped<IEnrichMoviesForVotingSessionWithPlaceholdersVisitor, MoviesVisitor>();
        
        services.AddScoped<IGetVotingSessionsMetadataVisitor, VotingSessionResultIdQueryVisitor>();
        services.AddScoped<IGetCurrentVotingSessionIdVisitor, VotingSessionResultIdQueryVisitor>();
        services.AddScoped<IGetCurrentVotingSessionStatusVisitor, VotingSessionResultIdQueryVisitor>();
        services.AddScoped<IGetVotingSessionResultVisitor, VotingSessionResultIdQueryVisitor>();

        services.AddScoped<IStartNewVotingVisitor, VotingSessionCommandVisitor>();
        services.AddScoped<IConcludeVotingVisitor, VotingSessionCommandVisitor>();

        services.AddScoped<IVotingSessionStatusMapperVisitor, VotingMapperVisitor>();
        services.AddScoped<IVotingSessionIdMapperVisitor, VotingMapperVisitor>();
        services.AddScoped<IVotingSessionsMapperVisitor, VotingMapperVisitor>();
        services.AddScoped<IAknowledgedMapperVisitor, VotingMapperVisitor>();

        services.AddScoped<IVoteVisitor, VoteVisitor>();

        services.AddSingleton<IVotingDeciderFactory, VotingDeciderFactory>();
        services.AddSingleton<IPickUserToNominateStrategyFactory, PickUserToNominateStrategyFactory>();

        return services;
    }
}