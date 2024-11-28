using Filmowanie.Voting.Interfaces;
using Filmowanie.Voting.Routes;
using Filmowanie.Voting.Visitors;
using Microsoft.Extensions.DependencyInjection;

namespace Filmowanie.Voting.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterVotingDomain(this IServiceCollection services)
    {
        services.AddScoped<IVotingSessionRoutes, VotingSessionRoutes>();

        services.AddScoped<IGetMoviesForVotingSessionVisitor, MoviesVisitor>();
        services.AddScoped<IEnrichMoviesForVotingSessionWithPlaceholdersVisitor, MoviesVisitor>();
        
        services.AddScoped<IGetCurrentVotingSessionVisitor, VotingSessionVisitor>();

        return services;
    }
}