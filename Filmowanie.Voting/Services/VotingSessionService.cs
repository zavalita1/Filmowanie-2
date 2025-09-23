using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.DTOs.Outgoing;
using Filmowanie.Voting.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Services;

// TODO UTs
internal sealed class VotingSessionService : IVotingSessionService
{
    private readonly IVotingResultsRepository votingSessionQueryRepository;
    private readonly ILogger<VotingSessionService> log;
    private readonly IMemoryCache memoryCache;

    private const string CacheKeyPrefix = "Winners-list";

    public VotingSessionService(IVotingResultsRepository votingSessionQueryRepository, ILogger<VotingSessionService> log, IMemoryCache memoryCache)
    {
        this.votingSessionQueryRepository = votingSessionQueryRepository;
        this.log = log;
        this.memoryCache = memoryCache;
    }

    public Task<Maybe<VotingSessionId?>> GetCurrentVotingSessionIdAsync(Maybe<DomainUser> maybeCurrentUser, CancellationToken cancelToken) =>
        maybeCurrentUser.AcceptAsync(GetCurrentVotingSessionId, this.log, cancelToken);

    public Task<Maybe<VotingSessionId>> GetLastVotingSessionIdAsync(Maybe<DomainUser> maybeCurrentUser, CancellationToken cancelToken) =>
        maybeCurrentUser.AcceptAsync(GetLastVotingSessionId, this.log, cancelToken);

    public Maybe<VotingSessionId> GetRequiredVotingSessionId(Maybe<VotingSessionId?> maybeCurrentVotingSessionId) =>
        maybeCurrentVotingSessionId.Accept(GetRequiredCurrentVotingSessionId, this.log);

    public Task<Maybe<MovieVotingStandingsListDTO>> GetMovieVotingStandingsList(Maybe<TenantId> input, CancellationToken cancelToken) =>
        input.AcceptAsync(GetMovieVotingStandingsList, this.log, cancelToken);

    public Task<Maybe<WinnerMetadata[]>> GetWinnersMetadataAsync(Maybe<VotingMetadata[]> maybeVotingMetadata, Maybe<TenantId> maybeTenant, CancellationToken cancelToken) =>
        maybeVotingMetadata.Merge(maybeTenant).AcceptAsync(GetWinnersMetadataAsync, this.log, cancelToken);

    public async Task<Maybe<WinnerMetadata[]>> GetWinnersMetadataAsync((VotingMetadata[], TenantId) input, CancellationToken cancelToken)
    {
        var winnersIds = input.Item1.Select(x => x.Winner.Id).ToHashSet();
        var cacheKey = $"{CacheKeyPrefix}-{winnersIds.GetHashCode()}";
        if (!this.memoryCache.TryGetValue(cacheKey, out var cached))
        {
            var results = await votingSessionQueryRepository.GetAllVotingResultsMetadataAsync(cancelToken);

            if (results.Error.HasValue)
                return new Error<WinnerMetadata[]>("Error during fetching voting results from db.", ErrorType.Unknown);

            var toCache = results.Result
                .ToDictionary(x => x.WinnerMovieId.Id, x => x.WinnerNominatedBy.Name);

            this.memoryCache.Set(cacheKey, toCache);
            cached = toCache;
        }

        if (cached is not Dictionary<string, string> typedValue)
            return new Error<WinnerMetadata[]>("Invalid cache object!", ErrorType.InvalidState);

        var result = input.Item1.Join(typedValue, x => x.Winner.Id, x => x.Key, (x, y) =>
                new WinnerMetadata(x.Winner.Id, x.Winner.Name, x.Winner.OriginalTitle, x.Winner.CreationYear, y.Value, x.Concluded, x.Winner.FilmwebUrl))
            .OrderByDescending(x => x.Watched)
            .ToArray();
        return result.AsMaybe();
    }

    public async Task<Maybe<VotingSessionId?>> GetCurrentVotingSessionId(DomainUser currentUser, CancellationToken cancelToken)
    {
        var votingSession = await votingSessionQueryRepository.GetUnconcludedResultAsync(cancelToken);

        if (votingSession.Result == null)
            return default(VotingSessionId?).AsMaybe(); // no current voting = voting has not been started yet.

        return votingSession.Map(x => Guid.Parse(x!.id)).Map(x => new VotingSessionId(x) as VotingSessionId?);
    }

    public async Task<Maybe<VotingSessionId>> GetLastVotingSessionId(DomainUser currentUser, CancellationToken cancelToken)
    {
        var votingSession = await votingSessionQueryRepository.GetLastNVotingResultsAsync(1, cancelToken);
        var id = votingSession.Map(x => x.Single());
        var guidId = id.Map(x => Guid.Parse(x.id));
        return guidId.Map(x => new VotingSessionId(x));
    }

    private static Maybe<VotingSessionId> GetRequiredCurrentVotingSessionId(VotingSessionId? input)
    {
        if (!input.HasValue)
            return new Error<VotingSessionId>("Voting has not started yet!", ErrorType.IncomingDataIssue);

        return input.Value.AsMaybe();
    }

    private async Task<Maybe<MovieVotingStandingsListDTO>> GetMovieVotingStandingsList(TenantId input, CancellationToken cancelToken)
    {
        var maybeVotingSession = await votingSessionQueryRepository.GetLastNVotingResultsAsync(10, cancelToken);

        if (maybeVotingSession.Error.HasValue)
            return maybeVotingSession.Error.Value.ChangeResultType<IEnumerable<IReadOnlyVotingResult>, MovieVotingStandingsListDTO>();

        var votingSessions = maybeVotingSession.Result!
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
