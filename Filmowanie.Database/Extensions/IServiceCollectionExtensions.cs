using Filmowanie.Database.Contexts;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Repositories;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Filmowanie.Database.Extensions;

public static class IServiceCollectionExtensions
{
    public static void RegisterDatabaseServices(this IServiceCollection services, IConfiguration configuration, Abstractions.Enums.Environment environment)
    {
        var dbConnectionString = configuration["dbConnectionString"]!;
        services.AddDbContext<IdentityDbContext>(options => options.UseCosmos(connectionString: dbConnectionString, databaseName: "db-filmowanie2"));
        services.AddDbContext<MoviesContext>(options => options.UseCosmos(connectionString: dbConnectionString, databaseName: "db-filmowanie2"));

        var dataProtectionBuilder = services.AddDataProtection().SetApplicationName("filmowanie2");

        if (environment != Abstractions.Enums.Environment.Development)
            dataProtectionBuilder.PersistKeysToDbContext<IdentityDbContext>();

        services.AddScoped<IUsersQueryRepository, UsersQueryRepository>();
        services.AddScoped<IUsersCommandRepository, UsersCommandRepository>();
    }
}