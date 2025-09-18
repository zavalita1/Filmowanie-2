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
    private readonly VotingResultsContext _ctx;
    private readonly CosmosOptions _options;

    public VotingSessionCommandRepository(VotingResultsContext ctx, IOptions<CosmosOptions> options)
    {
        _ctx = ctx;
        _options = options.Value;
    }

    public Task InsertAsync(IReadOnlyVotingResult votingResult, CancellationToken cancelToken)
    {
        var votingResultEntity = votingResult.AsMutable();
        _ctx.VotingResults.Add(votingResultEntity);
        return _ctx.SaveChangesAsync(cancelToken);
    }

    public async Task UpdateAsync(string id, Action<VotingResult> updateAction, CancellationToken cancelToken)
    {
        var votingResultEntity = await _ctx.VotingResults.AsNoTracking().SingleAsync(x => x.id == id, cancelToken);
        updateAction.Invoke(votingResultEntity);

        var cosmosClient = new CosmosClient(_options.ConnectionString);
        var c = cosmosClient.GetContainer(ServiceCollectionExtensions.DatabaseName, DbContainerNames.Entities);
        await c.ReplaceItemAsync(votingResultEntity, votingResultEntity.id, new PartitionKey(votingResultEntity.id), null, cancelToken);
    }
}