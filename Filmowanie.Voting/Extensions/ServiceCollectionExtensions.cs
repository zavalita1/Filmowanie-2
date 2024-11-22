using Microsoft.Extensions.DependencyInjection;

namespace Filmowanie.Voting.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterVotingDomain(this IServiceCollection services)
    {
        return services;
    }
}