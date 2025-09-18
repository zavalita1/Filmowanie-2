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
using Azure.Identity;
using System.Configuration;

namespace Filmowanie.Database.Extensions;

public static class ServiceCollectionExtensions
{
    public const string DatabaseName = "db-filmowanie2";

    public static void RegisterDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IVotingResultsRepository, VotingResultsRepository>();
        services.AddScoped<IRepositoryInUserlessContextProvider, RepositoryInUserlessContextProvider>();
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
        services.AddScoped<IVotingSessionQueryRepositoryInUserlessContextProvider, VotingSessionQueryRepositoryInUserlessContextProvider>();
        services.AddScoped<IMovieQueryRepositoryInUserslessContextProvider, VotingSessionQueryRepositoryInUserlessContextProvider>();
        services.AddScoped<IVotingSessionQueryRepository, VotingSessionQueryRepository>();
        services.Decorate<IVotingSessionQueryRepository, VotingSessionQueryRepositoryDecorator>();
        services.AddScoped<IVotingSessionCommandRepository, VotingSessionCommandRepository>();
        services.AddScoped<IMovieQueryRepository, MovieQueryRepository>();
        services.Decorate<IMovieQueryRepository, MovieQueryRepositoryDecorator>();

        services.AddScoped<IMovieCommandRepository, MovieCommandRepository>();

        services.AddScoped<IPushSubscriptionCommandRepository, PushSubscriptionCommandRepository>();
        services.AddScoped<IPushSubscriptionQueryRepository, PushSubscriptionQueryRepository>();
        services.Decorate<IPushSubscriptionQueryRepository, PushSubscriptionQueryDecorator>();
    }

    public static void PersistDataProtectionKeysLocal(this IServiceCollection services)
    {
        var dataProtectionBuilder = services.AddDataProtection().SetApplicationName("filmowanie2");

        var currentDll = Assembly.GetCallingAssembly().Location;
        var currentDir = Path.GetDirectoryName(currentDll)!;
        dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(currentDir));
    }
    
    public static void PersistDataProtectionKeysBlob(this IServiceCollection services, IConfiguration configuration)
    {
        var dataProtectionBuilder = services.AddDataProtection().SetApplicationName("filmowanie2");
        var credential = new DefaultAzureCredential();
        var blobUrl = configuration["DataProtectionKeysBlobUrl"];
        var keyVaultName = configuration["KeyVaultName"];
       // var keyId = configuration["https://kv-filmowanie2.vault.azure.net/keys/dataprotectionkey/3843c98c4a2a411892e010f92ac0f205"];

        if (string.IsNullOrEmpty(blobUrl))
            throw new ConfigurationErrorsException("Missing data-protection blob url!");

        if (string.IsNullOrEmpty(keyVaultName))
            throw new ConfigurationErrorsException("Missing keyvault name in config!");

        // if (string.IsNullOrEmpty(keyId))
        //     throw new ConfigurationErrorsException("Missing key identifier in config!");

        var kvUri = $"https://{keyVaultName}.vault.azure.net";

        dataProtectionBuilder
        .PersistKeysToAzureBlobStorage(new Uri(blobUrl), credential);
        //.ProtectKeysWithAzureKeyVault(new Uri(keyId), credential); TODO
    }
}