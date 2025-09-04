using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.DTOs.Outgoing;
using Filmowanie.Voting.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Services;

internal sealed class VotingSessionService : IVotingSessionService
{
    private readonly IVotingSessionQueryRepository _votingSessionQueryRepository;
    private readonly ILogger<VotingSessionService> _log;
    private readonly IMemoryCache _memoryCache;

    private const string CacheKeyPrefix = "Winners-list";

    public VotingSessionService(IVotingSessionQueryRepository votingSessionQueryRepository, ILogger<VotingSessionService> log, IMemoryCache memoryCache)
    {
        _votingSessionQueryRepository = votingSessionQueryRepository;
        _log = log;
        _memoryCache = memoryCache;
    }

    public Task<Maybe<IReadOnlyVotingResult?>> GetCurrentVotingSession(Maybe<DomainUser> maybeCurrentUser, CancellationToken cancellationToken) =>
        maybeCurrentUser.AcceptAsync(GetCurrentVotingSession, _log, cancellationToken);

    public Task<Maybe<VotingSessionId?>> GetCurrentVotingSessionIdAsync(Maybe<DomainUser> maybeCurrentUser, CancellationToken cancellationToken) =>
        maybeCurrentUser.AcceptAsync(GetCurrentVotingSessionId, _log, cancellationToken);

    public Maybe<VotingSessionId> GetRequiredVotingSessionId(Maybe<VotingSessionId?> maybeCurrentVotingSessionId) =>
        maybeCurrentVotingSessionId.Accept(GetRequiredCurrentVotingSessionId, _log);

    public async Task<Maybe<IReadOnlyVotingResult?>> GetCurrentVotingSession(DomainUser currentUser, CancellationToken cancellationToken)
    {
        var currentVotingResults = await _votingSessionQueryRepository.Get(x => x.Concluded == null, currentUser.Tenant, cancellationToken);
        return currentVotingResults.AsMaybe();
    }

    public Task<Maybe<MovieVotingStandingsListDTO>> GetMovieVotingStandingsList(Maybe<TenantId> input, CancellationToken cancellationToken) =>
        input.AcceptAsync(GetMovieVotingStandingsList, _log, cancellationToken);

    public Task<Maybe<WinnerMetadata[]>> GetWinnersMetadataAsync(Maybe<(VotingMetadata[], TenantId)> input, CancellationToken cancellationToken) =>
        input.AcceptAsync(GetWinnersMetadataAsync, _log, cancellationToken);

    public async Task<Maybe<WinnerMetadata[]>> GetWinnersMetadataAsync((VotingMetadata[], TenantId) input, CancellationToken cancellationToken)
    {
        var winnersIds = input.Item1.Select(x => x.Winner.Id).ToHashSet();
        var cacheKey = $"{CacheKeyPrefix}-{winnersIds.GetHashCode()}";
        if (!_memoryCache.TryGetValue(cacheKey, out var cached))
        {
            var results = await _votingSessionQueryRepository.Get(
                x => true,
                x => x, input.Item2,
                cancellationToken);
            var toCache = results
                .SelectMany(x => x.MoviesAdded.Where(y => winnersIds.Contains(y.Movie.id)))
                .GroupBy(x => x.Movie.id)
                .ToDictionary(x => x.Key, x => x.MaxBy(y => y.NominationConcluded)!.NominatedBy.Name);

            _memoryCache.Set(cacheKey, toCache);
            cached = toCache;
        }

        if (cached is not Dictionary<string, string> typedValue)
            return new Error("Invalid cache object!", ErrorType.InvalidState).AsMaybe<WinnerMetadata[]>();

        var result = input.Item1.Join(typedValue, x => x.Winner.Id, x => x.Key, (x, y) =>
                new WinnerMetadata(x.Winner.Id, x.Winner.Name, x.Winner.OriginalTitle, x.Winner.CreationYear, y.Value, x.Concluded))
            .OrderByDescending(x => x.Watched)
            .ToArray();
        return result.AsMaybe();
    }

    public async Task<Maybe<VotingSessionId?>> GetCurrentVotingSessionId(DomainUser currentUser, CancellationToken cancellationToken)
    {
        var votingSession = await GetCurrentVotingSession(currentUser, cancellationToken);

        if (votingSession.Result == null)
            return default(VotingSessionId?).AsMaybe(); // no current voting = voting has not been started yet.

        return votingSession.Map(x => Guid.Parse(x!.id)).Map(x => new VotingSessionId(x) as VotingSessionId?);
    }

    private static Maybe<VotingSessionId> GetRequiredCurrentVotingSessionId(VotingSessionId? input)
    {
        if (!input.HasValue)
            return new Maybe<VotingSessionId>(default, new Error("Voting has not started yet!", ErrorType.IncomingDataIssue));

        return input.Value.AsMaybe();
    }

    private async Task<Maybe<MovieVotingStandingsListDTO>> GetMovieVotingStandingsList(TenantId input, CancellationToken cancellationToken)
    {
        var votingSessions = (await _votingSessionQueryRepository.Get(x => x.Concluded != null, x => x, input, cancellationToken))
            .Reverse()
            .Take(10)
            .Select(x => new VotingSessionData(x, GetPlaces(x).ToArray()))
            .ToArray();

        AdjustWithExAequo(votingSessions);

        var movies = votingSessions.SelectMany(x => x.Data.Movies).ToArray();

        var resultRows = new List<MovieVotingStandingsDTO>(10);
        foreach (var movie in movies)
        {
            var places = votingSessions.Select(x => x.Places.SingleOrDefault(y => y.Data.Movie.id == movie.Movie.id)?.Place).ToArray();
            var resultRow = new MovieVotingStandingsDTO(movie.Movie.Name, places, []);
            resultRows.Add(resultRow);
        }

        var movieVotingStandingsListDto = new MovieVotingStandingsListDTO(resultRows.ToArray());
        return new Maybe<MovieVotingStandingsListDTO>(movieVotingStandingsListDto, null);
    }

    private static void AdjustWithExAequo(VotingSessionData[] toAdjust)
    {
        foreach (var votingSessionData in toAdjust)
        {
            for (var i = 1; i < votingSessionData.Places.Length; i++)
            {
                if (votingSessionData.Places[i - 1].Data.VotingScore == votingSessionData.Places[i].Data.VotingScore)
                {
                    votingSessionData.Places[i].Place = votingSessionData.Places[i - 1].Place;
                }
            }
        }
    }

    private static IEnumerable<VotingSessionPlacesData> GetPlaces(IReadOnlyVotingResult result)
    {
        var moviesSorted = result.Movies.OrderByDescending(x => x.VotingScore);
        var counter = 0;
        foreach (var movie in moviesSorted)
        {
            yield return new VotingSessionPlacesData(movie, ++counter);
        }
    }


    private readonly record struct VotingSessionData(IReadOnlyVotingResult Data, VotingSessionPlacesData[] Places);

    private record VotingSessionPlacesData(IReadOnlyEmbeddedMovieWithVotes Data, int Place)
    {
        public int Place { get; set; } = Place;
    }

}
