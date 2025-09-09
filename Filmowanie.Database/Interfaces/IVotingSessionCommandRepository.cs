using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Database.Interfaces;

internal interface IVotingSessionCommandRepository
{
    public Task InsertAsync(IReadOnlyVotingResult votingResult, CancellationToken cancelToken);

    Task UpdateAsync(string id, Action<VotingResult> updateAction, CancellationToken cancelToken);
}