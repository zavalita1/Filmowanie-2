using Filmowanie.Database.Contexts;
using Filmowanie.Database.Extensions;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Microsoft.EntityFrameworkCore;

namespace Filmowanie.Database.Repositories;

internal sealed class VotingSessionCommandRepository : IVotingSessionCommandRepository
{
    private readonly VotingResultsContext _ctx;

    public VotingSessionCommandRepository(VotingResultsContext ctx)
    {
        _ctx = ctx;
    }

    public Task InsertAsync(IReadOnlyVotingResult votingResult, CancellationToken cancellationToken)
    {
        var votingResultEntity = votingResult.AsMutable();
        _ctx.VotingResults.Add(votingResultEntity);
        return _ctx.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(string id, IEnumerable<IReadOnlyEmbeddedMovieWithVotes> movies, IEnumerable<IReadOnlyEmbeddedUserWithNominationAward> usersAwards, DateTime concluded, IEnumerable<IReadOnlyEmbeddedMovieWithNominationContext> moviesAdded, IReadOnlyEmbeddedMovie winner, CancellationToken cancellationToken)
    {
        var votingResultEntity = await _ctx.VotingResults.SingleAsync(x => x.id == id, cancellationToken);

        votingResultEntity.Movies = movies.Select(IReadOnlyEntitiesExtensions.AsMutable).ToArray();
        votingResultEntity.Concluded = concluded;
        votingResultEntity.UsersAwardedWithNominations = usersAwards.Select(IReadOnlyEntitiesExtensions.AsMutable).ToArray();
        votingResultEntity.MoviesAdded = moviesAdded.Select(IReadOnlyEntitiesExtensions.AsMutable).ToArray();
        votingResultEntity.Winner = winner.AsMutable();

        await _ctx.SaveChangesAsync(cancellationToken);
    }
}