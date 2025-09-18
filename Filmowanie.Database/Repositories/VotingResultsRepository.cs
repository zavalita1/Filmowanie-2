using System.Linq.Expressions;
using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Database.Repositories;

internal sealed class VotingResultsRepository : IVotingResultsRepository
{
    private readonly IVotingSessionQueryRepository _repository;
    private readonly ILogger<VotingResultsRepository> _logger;

    public VotingResultsRepository(IVotingSessionQueryRepository repository, ILogger<VotingResultsRepository> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Maybe<IReadOnlyVotingResult?>> GetByIdAsync(VotingSessionId id, CancellationToken cancelToken)
    {
        var votingResult = await _repository.Get(x => x.id == id.CorrelationId.ToString(), cancelToken);
        return votingResult.AsMaybe();
    }

    public async Task<Maybe<IReadOnlyVotingResult?>> GetUnconcludedResultAsync(CancellationToken cancelToken)
    {
        try
        {
            var result = await _repository.Get(x => x.Concluded == null, cancelToken);
            return result.AsMaybe();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when trying to get current voting!");
            return new Error<IReadOnlyVotingResult?>(ex.Message, ErrorType.Unknown);
        }
    }

    public async Task<Maybe<IEnumerable<IReadOnlyVotingResult>>> GetLastNVotingResultsAsync(int numberOfResultsToReturn, CancellationToken cancelToken)
    {
        try
        {
            Expression<Func<IReadOnlyVotingResult, bool>> query = x => x.Concluded != null;
            var result = await _repository.GetVotingResultAsync(query, x => x.Concluded!, -1 * numberOfResultsToReturn, cancelToken);
            var materializedResult = result.ToArray();

            if (materializedResult.Length != numberOfResultsToReturn)
                return new Error<IEnumerable<IReadOnlyVotingResult>>($"Expected to find {numberOfResultsToReturn} last voting results, but only {materializedResult.Length} were found in db!",
                    ErrorType.IncomingDataIssue);

            return materializedResult.AsMaybe<IEnumerable<IReadOnlyVotingResult>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during fetching voting results");
            return new Error<IEnumerable<IReadOnlyVotingResult>>(ex.ToString(), ErrorType.Unknown);
        }
    }

    public async Task<Maybe<IEnumerable<IReadOnlyVotingResultMetadata>>> GetAllVotingResultsMetadataAsync(CancellationToken cancelToken)
    {
        try
        {
            var result = (await _repository.GetAll(x => x.Concluded != null, cancelToken)).ToArray();

            var materializedResult = result.Select(IReadOnlyVotingResultMetadata (x) => new ReadOnlyVotingResultMetadata(x.Concluded!.Value, new MovieId(x.Winner!.Movie.id), new VotingSessionId(Guid.Parse(x.id)), new DomainUser(x.Winner.NominatedBy.id, x.Winner.NominatedBy.Name, false, false, new TenantId(1), DateTime.UnixEpoch, Gender.Unspecified)));
            return materializedResult.AsMaybe();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during fetching voting results");
            return new Error<IEnumerable<IReadOnlyVotingResultMetadata>>(ex.ToString(), ErrorType.Unknown);
        }
    }
}