using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Database.Interfaces;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Visitors;

internal sealed class WinnersMetadataMapperVisitor : IWinnersMetadataMapperVisitor
{
    private readonly ILogger<WinnersMetadataMapperVisitor> _log;
    private readonly IVotingSessionQueryRepository _sessionQueryRepository;
    private readonly IMemoryCache _memoryCache;

    private const string CacheKeyPrefix = "Winners-list";

    public WinnersMetadataMapperVisitor(ILogger<WinnersMetadataMapperVisitor> log, IVotingSessionQueryRepository sessionQueryRepository, IMemoryCache memoryCache)
    {
        _log = log;
        _sessionQueryRepository = sessionQueryRepository;
        _memoryCache = memoryCache;
    }

    public async Task<OperationResult<WinnerMetadata[]>> VisitAsync(OperationResult<(VotingMetadata[], TenantId)> input, CancellationToken cancellationToken)
    {
        var winnersIds = input.Result.Item1.Select(x => x.Winner.Id).ToHashSet();
        var cacheKey = $"{CacheKeyPrefix}-{winnersIds.GetHashCode()}";
        if (!_memoryCache.TryGetValue(cacheKey, out var cached))
        {
            var results = await _sessionQueryRepository.Get(
                x => true,
                x => x, input.Result.Item2,
                cancellationToken);
            var toCache = results
                .SelectMany(x => x.MoviesAdded.Where(y => winnersIds.Contains(y.Movie.id)))
                .GroupBy(x => x.Movie.id)
                .ToDictionary(x => x.Key, x => x.MaxBy(y => y.NominationConcluded)!.NominatedBy.Name);

            _memoryCache.Set(cacheKey, toCache);
            cached = toCache;
        }

        if (cached is not Dictionary<string, string> typedValue)
            return new OperationResult<WinnerMetadata[]>(default, new Error("Invalid cache object!", ErrorType.InvalidState));

        var result = input.Result.Item1.Join(typedValue, x => x.Winner.Id, x => x.Key, (x, y) => 
            new WinnerMetadata(x.Winner.Id, x.Winner.Name, x.Winner.OriginalTitle, x.Winner.CreationYear, y.Value, x.Concluded))
            .OrderByDescending(x => x.Watched)
            .ToArray();
        return new OperationResult<WinnerMetadata[]>(result, null);
    }

    public ILogger Log => _log;
}