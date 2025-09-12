using Filmowanie.Database.Contexts;
using Filmowanie.Database.Decorators;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Repositories;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Filmowanie.Database.Repositories.Internal;

namespace Filmowanie.Database.Extensions;

public static class ServiceCollectionExtensions
{
    private const string DatabaseName = "db-filmowanie2";

    public static void RegisterDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IVotingResultsRepository, VotingResultsRepository>();
        services.AddScoped<IVotingResultsCommandRepository, VotingResultsCommandRepository>();
        services.AddScoped<IDomainUsersRepository, DomainUsersRepository>();
        services.AddScoped<IMovieDomainRepository, MovieDomainRepository>();

        var dbConnectionString = configuration["dbConnectionString"]!;
        services.AddDbContext<IdentityDbContext>(options => options.UseCosmos(connectionString: dbConnectionString, databaseName: DatabaseName));
        services.AddDbContext<MoviesContext>(options => options.UseCosmos(connectionString: dbConnectionString, databaseName: DatabaseName));
        services.AddDbContext<VotingResultsContext>(options => options.UseCosmos(connectionString: dbConnectionString, databaseName: DatabaseName));
        services.AddDbContext<PushSubscriptionsContext>(options => options.UseCosmos(connectionString: dbConnectionString, databaseName: DatabaseName));

        services.AddScoped<IUsersQueryRepository, UsersQueryRepository>();

        services.AddScoped<IUsersCommandRepository, UsersCommandRepository>();
        services.AddScoped<IVotingSessionQueryRepository, VotingSessionQueryRepository>();
        services.Decorate<IVotingSessionQueryRepository, VotingSessionQueryRepositoryDecorator>();
        services.AddScoped<IVotingSessionCommandRepository, VotingSessionCommandRepository>();
        services.AddScoped<IMovieQueryRepository, MovieQueryRepository>();
        services.Decorate<IMovieQueryRepository, MovieQueryRepositoryDecorator>();

        services.AddScoped<IMovieCommandRepository, MovieCommandRepository>();

        services.AddScoped<IPushSubscriptionCommandRepository, PushSubscriptionCommandRepository>();
        services.AddScoped<IPushSubscriptionQueryRepository, PushSubscriptionQueryRepository>();
        services.Decorate<IPushSubscriptionQueryRepository, PushSubscriptionQueryDecorator>();
        
        PersistDataProtectionKeys(services);
    }

    private static void PersistDataProtectionKeys(IServiceCollection services)
    {
        var dataProtectionBuilder = services.AddDataProtection().SetApplicationName("filmowanie2");
        
        var currentDll = Assembly.GetCallingAssembly().Location;
        var currentDir = Path.GetDirectoryName(currentDll)!;
        dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(currentDir));
    }
}