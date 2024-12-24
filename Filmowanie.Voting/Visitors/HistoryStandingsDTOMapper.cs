using Filmowanie.Abstractions;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.DTOs.Outgoing;
using Filmowanie.Voting.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Visitors;

internal sealed class HistoryStandingsDTOMapper : IHistoryStandingsDTOMapperVisitor
{
    private readonly IVotingSessionQueryRepository _votingSessionQueryRepository;
    private readonly ILogger<HistoryStandingsDTOMapper> _log;

    public HistoryStandingsDTOMapper(IVotingSessionQueryRepository votingSessionQueryRepository, ILogger<HistoryStandingsDTOMapper> log)
    {
        _votingSessionQueryRepository = votingSessionQueryRepository;
        _log = log;
    }

    public async Task<OperationResult<MovieVotingStandingsListDTO>> VisitAsync(OperationResult<TenantId> input, CancellationToken cancellationToken)
    {
        var votingSessions = (await _votingSessionQueryRepository.Get(x => x.Concluded != null, x => x, input.Result, cancellationToken))
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
        return new OperationResult<MovieVotingStandingsListDTO>(movieVotingStandingsListDto, null);
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

    private static IEnumerable<VotingSessionPlacesData> GetPlaces(IReadonlyVotingResult result)
    {
        var moviesSorted = result.Movies.OrderByDescending(x => x.VotingScore);
        var counter = 0;
        foreach (var movie in moviesSorted)
        {
            yield return new VotingSessionPlacesData(movie, ++counter);
        }
    }


    private readonly record struct VotingSessionData(IReadonlyVotingResult Data, VotingSessionPlacesData[] Places);

    private record VotingSessionPlacesData(IReadOnlyEmbeddedMovieWithVotes Data, int Place)
    {
        public int Place { get; set; } = Place;
    }

    public ILogger Log => _log;
}