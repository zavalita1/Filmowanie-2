using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Extensions;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Database.Repositories;

internal sealed class VotingResultsCommandRepository : IVotingResultsCommandRepository
{
    private readonly IVotingSessionCommandRepository repository;
    private readonly ILogger<VotingResultsCommandRepository> logger;

    public VotingResultsCommandRepository(IVotingSessionCommandRepository repository, ILogger<VotingResultsCommandRepository> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Maybe<VoidResult>> UpdateAsync(VotingSessionId id, IEnumerable<IReadOnlyEmbeddedMovieWithVotes> movies, IEnumerable<IReadOnlyEmbeddedUserWithNominationAward> usersAwards, DateTime concluded,
        IEnumerable<IReadOnlyEmbeddedMovieWithNominationContext> moviesAdded, IReadOnlyEmbeddedMovieWithNominatedBy winner, IEnumerable<IReadOnlyEmbeddedMovie> moviesToRemove, CancellationToken cancelToken)
    {
        var updateFunc = (VotingResult votingResultEntity) =>
        {
            votingResultEntity.Movies = movies.Select(IReadOnlyEntitiesExtensions.AsMutable).ToList();
            votingResultEntity.Concluded = concluded;
            votingResultEntity.UsersAwardedWithNominations = usersAwards.Select(IReadOnlyEntitiesExtensions.AsMutable).ToList();
            votingResultEntity.MoviesAdded = moviesAdded.Select(IReadOnlyEntitiesExtensions.AsMutable).ToList();
            votingResultEntity.Winner = winner.AsMutable();
            votingResultEntity.MoviesGoingByeBye = moviesToRemove.Select(IReadOnlyEntitiesExtensions.AsMutable).ToList();
        };

        try
        {
            await this.repository.UpdateAsync(id.CorrelationId.ToString(), updateFunc, cancelToken);
            return VoidResult.Void;
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Error when updating voting result entity!");
            return new Error<VoidResult>(e.Message, ErrorType.Unknown);
        }
    }

    public Task InsertAsync(IReadOnlyVotingResult votingResult, CancellationToken cancelToken) => this.repository.InsertAsync(votingResult, cancelToken);

    public async Task<Maybe<VoidResult>> ResetAsync(VotingSessionId id, CancellationToken cancelToken)
    {
         var updateFunc = (VotingResult votingResultEntity) =>
        {
            votingResultEntity.Movies = new List<EmbeddedMovieWithVotes>();
            votingResultEntity.Concluded = null;
            votingResultEntity.UsersAwardedWithNominations = new List<EmbeddedUserWithNominationAward>();
            votingResultEntity.MoviesAdded = new List<EmbeddedMovieWithNominationContext>();
            votingResultEntity.Winner = null;
            votingResultEntity.MoviesGoingByeBye = new List<EmbeddedMovie>();
        };

        try
        {
            await this.repository.UpdateAsync(id.CorrelationId.ToString(), updateFunc, cancelToken);
            return VoidResult.Void;
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Error when updating voting result entity!");
            return new Error<VoidResult>(e.Message, ErrorType.Unknown);
        }
    }
}