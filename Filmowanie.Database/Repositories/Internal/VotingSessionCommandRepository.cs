using Filmowanie.Abstractions.Configuration;
using Filmowanie.Database.Contants;
using Filmowanie.Database.Contexts;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Extensions;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Filmowanie.Database.Repositories.Internal;

internal sealed class VotingSessionCommandRepository : IVotingSessionCommandRepository
{
    private readonly VotingResultsContext ctx;
    private readonly CosmosOptions options;
    private readonly ICosmosClientOptionsProvider cosmosClientOptionsProvider;


    public VotingSessionCommandRepository(VotingResultsContext ctx, IOptions<CosmosOptions> options, ICosmosClientOptionsProvider cosmosClientOptionsProvider)
    {
        this.ctx = ctx;
        this.options = options.Value;
        this.cosmosClientOptionsProvider = cosmosClientOptionsProvider;
    }

    public Task InsertAsync(IReadOnlyVotingResult votingResult, CancellationToken cancelToken)
    {
        var votingResultEntity = votingResult.AsMutable();
        ctx.VotingResults.Add(votingResultEntity);
        return ctx.SaveChangesAsync(cancelToken);
    }

    public async Task UpdateAsync(string id, Action<VotingResult> updateAction, CancellationToken cancelToken)
    {
        var votingResultEntity = await ctx.VotingResults.AsNoTracking().SingleAsync(x => x.id == id, cancelToken);
        updateAction.Invoke(votingResultEntity);

        var cosmosClientOptions = this.cosmosClientOptionsProvider.Get();
        var cosmosClient = ClientInstance.GetClient(this.options.ConnectionString, cosmosClientOptions.ClientOptions);
        var c = cosmosClient.GetContainer(this.options.DbName, DbContainerNames.Entities);
        await c.ReplaceItemAsync(votingResultEntity, votingResultEntity.id, new PartitionKey(votingResultEntity.id), null, cancelToken);
    }

    private static class ClientInstance
    {
        private static CosmosClient? _client;
        private static readonly object Locker = new();

        public static CosmosClient GetClient(string connectionString, CosmosClientOptions options)
        {
            if (_client != null) return _client;

            lock (Locker)
            {
                _client = new CosmosClient(connectionString, options);
                return _client;
            }
        }
    }
}