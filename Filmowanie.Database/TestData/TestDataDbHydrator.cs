using Filmowanie.Abstractions.Configuration;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.OptionsProviders;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using System.Net;
using System.Reflection;
using System.Text.Json;

namespace Filmowanie.Database.TestData;

internal static class TestDataDbHydrator
{
    public static async Task HydrateDbWithMockedDataAsync(string databaseName, string dbConnectionString, string extensiveLoggingEnabled)
    {
        var container = CosmosDbContainerInstance.Instance;

        if (container == null)
            throw new InvalidOperationException("Container instance needs to be available for this to run!");

        var clientOptionsProvider = new EmulatedCosmosClientOptionsProvider(container.HttpClient, new LocalCosmosOptions(dbConnectionString, databaseName, bool.Parse(extensiveLoggingEnabled)));
        var clientOptions = clientOptionsProvider.Get();
        var cosmosClient = new CosmosClient(dbConnectionString, clientOptions.ClientOptions);

        var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);
        if (database.StatusCode == HttpStatusCode.OK)
        {
            Console.WriteLine("Db already exists. This means we're reusing cosmos container from the last run. If you want to run hydration process again, you need to manually drop the container.");
            return;
        }

        try
        {
            await HydrateDbWithMockedDataInnerAsync(database);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error occurred during data hydration : " + ex.Message);
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine("Stopping the container, as it shouldn't be reused. You should delete this container manually. Doing it automatically is restricted by testcontainers :(.");
            await container.DisposeAsync();
            throw;
        }
    }

    private static async Task HydrateDbWithMockedDataInnerAsync(DatabaseResponse database)
    {
        Console.WriteLine("Starting hydrating db with mocked data...");
        var currentDll = Assembly.GetExecutingAssembly().Location;
        var currentDir = Path.GetDirectoryName(currentDll);
        var entities = await File.ReadAllTextAsync($"{currentDir}/TestData/Entities.json");
        Console.WriteLine("Read entities file.");
        var events = await File.ReadAllTextAsync($"{currentDir}/TestData/Events.json");
        Console.WriteLine("Read events file.");

        Console.WriteLine("Created db");

        var entitiesContainer = await database.Database.CreateContainerIfNotExistsAsync(
            "Entities",
            "/id"
        );
        Console.WriteLine("Created entities container.");

        var eventsContainer = await database.Database.CreateContainerIfNotExistsAsync(
            "Events",
            "/id"
        );

        Console.WriteLine("Created events container.");

        var entityDocs = JsonDocument.Parse(entities);
        var entityCounter = 0;
        
        foreach (var doc in entityDocs.RootElement.EnumerateArray())
        {
            Console.WriteLine($"Writing entity: {++entityCounter}.");
            try
            {
                var type = doc.GetProperty("Type");
                if (type.GetString() == "UserEntity")
                {
                    Console.WriteLine($"user entity found ({doc.GetProperty("id").GetString()}). Writing...");
                    var obj = doc.Deserialize<UserEntity>();
                    await entitiesContainer.Container.CreateItemAsync(obj);
                    Console.WriteLine("user entity written.");
                }
                else if (type.GetString() == "VotingResult")
                {
                    Console.WriteLine($"voting result entity found ({doc.GetProperty("id").GetString()}). Writing...");
                    var obj = doc.Deserialize<VotingResult>();
                    await entitiesContainer.Container.CreateItemAsync(obj);
                    Console.WriteLine("voting result entity written.");
                }
                else if (type.GetString() == "MovieEntity")
                {
                    Console.WriteLine($"movie entity found ({doc.GetProperty("id").GetString()}). Writing...");
                    var obj = doc.Deserialize<MovieEntity>();
                    await entitiesContainer.Container.CreateItemAsync(obj);
                    Console.WriteLine("movie entity written.");
                }
                else
                    throw new Exception("Unknown type among entities!");
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                Console.WriteLine("Document already exists, skip");
                continue;
            }
            Console.WriteLine($"Wrote entity: {++entityCounter}.");
        }

        var eventDocs = JsonDocument.Parse(events);
        entityCounter = 0;
        
        foreach (var doc in eventDocs.RootElement.EnumerateArray())
        {
            Console.WriteLine($"Writing event: {++entityCounter}.");
            try
            {
                if (!doc.TryGetProperty("Type", out var type))
                {
                    Console.WriteLine("saga instance found. Writing...");
                    var obj = doc.ToString();
                    var idProp = doc.GetProperty("id");
                    using var ms = new MemoryStream();
                    using var w = new StreamWriter(ms);
                    await w.WriteAsync(obj);
                    await w.FlushAsync();
                    ms.Position = 0;

                    await eventsContainer.Container.CreateItemStreamAsync(ms, new PartitionKey(idProp.GetString()));
                    Console.WriteLine("sava instance written.");
                }
                else if (type.GetString() == "CanNominateMovieAgainEvent")
                {
                    Console.WriteLine($"can re-nominate event found ({doc.GetProperty("id").GetString()}). Writing...");
                    var obj = doc.Deserialize<CanNominateMovieAgainEvent>();
                    await eventsContainer.Container.CreateItemAsync(obj);
                    Console.WriteLine("can re-nominate event written.");
                }
                else if (type.GetString() == "NominatedMovieEvent")
                {
                    Console.WriteLine($"nominated event found ({doc.GetProperty("id").GetString()}). Writing...");
                    var obj = doc.Deserialize<NominatedMovieEvent>();
                    await eventsContainer.Container.CreateItemAsync(obj);
                    Console.WriteLine("nominated event entity written.");
                }
                else
                    throw new Exception("Unknown type among events!");
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                Console.WriteLine("Document already exists, skip");
                continue;
            }
            Console.WriteLine($"Wrote entity: {++entityCounter}.");
        }
    }
    
    private sealed class LocalCosmosOptions : IOptions<CosmosOptions>
    {
        public LocalCosmosOptions(string dbConnectionString, string dbName, bool extensiveLoggingEnabled)
        {
            Value = new() { ConnectionString = dbConnectionString, ExtensiveEFLoggingEnabled = extensiveLoggingEnabled, DbName =  dbName };
        }

        public CosmosOptions Value { get; }
    }
}