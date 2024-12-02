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

        services.AddScoped<IVotingSessionRoutes, VotingSessionRoutes>();
        services.AddScoped<IAdminVotingSessionRoutes, AdminVotingSessionRoutes>();

        services.AddScoped<IGetMoviesForVotingSessionVisitor, MoviesVisitor>();
        services.AddScoped<IEnrichMoviesForVotingSessionWithPlaceholdersVisitor, MoviesVisitor>();
        
        services.AddScoped<IGetCurrentVotingSessionIdVisitor, VotingSessionIdQueryVisitor>();
        services.AddScoped<IGetCurrentVotingSessionStatusVisitor, VotingSessionIdQueryVisitor>();
        services.AddScoped<IStartNewVotingVisitor, VotingSessionCommandVisitor>();
        services.AddScoped<IConcludeVotingVisitor, VotingSessionCommandVisitor>();
        services.AddScoped<IVotingSessionStatusMapperVisitor, MapperMapperVisitor>();
        services.AddScoped<IAknowledgedMapperVisitor, MapperMapperVisitor>();
        services.AddScoped<IVoteVisitor, VoteVisitor>();

        services.AddSingleton<IVotingDeciderFactory, VotingDeciderFactory>();
        services.AddSingleton<IPickUserToNominateStrategyFactory, PickUserToNominateStrategyFactory>();

        return services;
    }
}