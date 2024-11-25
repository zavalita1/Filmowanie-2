using Filmowanie.Database.Contexts;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Repositories;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Filmowanie.Database.Extensions;

public static class IServiceCollectionExtensions
{
    public static void RegisterDatabaseServices(this IServiceCollection services, IConfiguration configuration, Abstractions.Enums.Environment environment)
    {
        var dbConnectionString = configuration["dbConnectionString"]!;
        services.AddDbContext<IdentityDbContext>(options => options.UseCosmos(connectionString: dbConnectionString, databaseName: "db-filmowanie2"));
        services.AddDbContext<MoviesContext>(options => options.UseCosmos(connectionString: dbConnectionString, databaseName: "db-filmowanie2"));
        services.AddDbContext<EventsContext>(options => options.UseCosmos(connectionString: dbConnectionString, databaseName: "db-filmowanie2"));

        var dataProtectionBuilder = services.AddDataProtection().SetApplicationName("filmowanie2");
        
        var currentDll = Assembly.GetCallingAssembly().Location;
        var currentDir = Path.GetDirectoryName(currentDll)!;
        dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(currentDir));

        services.AddScoped<IUsersQueryRepository, UsersQueryRepository>();
        services.AddScoped<IUsersCommandRepository, UsersCommandRepository>();
        services.AddScoped<IEventsQueryRepository, EventsQueryRepository>();
    }
}