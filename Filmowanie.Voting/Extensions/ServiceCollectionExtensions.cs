using Filmowanie.Abstractions.Interfaces;
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

        services.AddScoped<IGetMoviesForVotingSessionVisitor, MoviesVisitor>();
        services.AddScoped<IEnrichMoviesForVotingSessionWithPlaceholdersVisitor, MoviesVisitor>();
        
        services.AddScoped<IGetCurrentVotingSessionVisitor, VotingSessionVisitor>();
        services.AddScoped<IGetCurrentVotingSessionStatusVisitor, VotingSessionVisitor>();
        services.AddScoped<IVotingSessionStatusVisitor, IVotingSessionStatusStatusMapperVisitor>();
        services.AddScoped<IVoteVisitor, VoteVisitor>();

        return services;
    }
}