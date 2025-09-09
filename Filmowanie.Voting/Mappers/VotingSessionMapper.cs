using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.DTOs.Outgoing;
using Microsoft.Extensions.Logging;
using System.Globalization;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Voting.Interfaces;
using Filmowanie.Abstractions.Helpers;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Voting.Mappers;

internal sealed class VotingSessionMapper : IVotingSessionMapper
{
    private readonly ILogger<VotingSessionMapper> _log;
    private readonly IVotingSessionService _votingSessionService;

    public VotingSessionMapper(ILogger<VotingSessionMapper> log, IVotingSessionService votingSessionService)
    {
        _log = log;
        _votingSessionService = votingSessionService;
    }

    public Maybe<VotingState> Map(Maybe<VotingSessionId?> input) => input.Accept(Map, _log);

    public Task<Maybe<VotingSessionId?>> MapAsync(Maybe<(string, DomainUser)> input, CancellationToken cancelToken) => input.AcceptAsync(MapAsync, _log, cancelToken);

    public Maybe<VotingSessionsDTO> Map(Maybe<VotingMetadata[]> input) => input.Accept(Map, _log);

    public Maybe<HistoryDTO> Map(Maybe<WinnerMetadata[]> input) => input.Accept(Map, _log);
    
    public Maybe<MovieDTO[]> Map(Maybe<(IReadOnlyMovieEntity[] MoviesEntities, IReadOnlyEmbeddedMovieWithVotes[] EmbeddedMovies, DomainUser CurrentUser)> input) => input.Accept(Map, _log);
   
    private Maybe<MovieDTO[]> Map((IReadOnlyMovieEntity[] MoviesEntities, IReadOnlyEmbeddedMovieWithVotes[] EmbeddedMovies, DomainUser CurrentUser) input)
    {
        var movies = input.EmbeddedMovies.Join(input.MoviesEntities, x => x.Movie.id, x => x.id, (x, y) => new { Movie = y, x.Votes });

        var resultMovies = new List<MovieDTO>(input.EmbeddedMovies.Length);

        foreach (var movie in movies)
        {
            _log.LogInformation($"Mapping movie: {movie.Movie.Name}");
            var votes = (int?)movie.Votes.SingleOrDefault(x => x.User.id == input.CurrentUser.Id)?.VoteType ?? 0;
            var duration = StringHelper.GetDurationString(movie.Movie.DurationInMinutes);
            var movieDto = new MovieDTO(movie.Movie.id, movie.Movie.Name, votes, movie.Movie.PosterUrl, movie.Movie.Description, movie.Movie.FilmwebUrl, movie.Movie.CreationYear, duration, movie.Movie.Genres, movie.Movie.Actors,
                movie.Movie.Directors, movie.Movie.Writers, movie.Movie.OriginalTitle);

            resultMovies.Add(movieDto);
        }

        return resultMovies.ToArray().AsMaybe();
    }


    private Maybe<HistoryDTO> Map(WinnerMetadata[] input)
    {
        var entries = input
            .Select(x => new HistoryEntryDTO(x.Name, x.OriginalTitle, x.CreationYear, x.NominatedBy, x.Watched.ToString("d", new CultureInfo("pl"))))
            .ToArray();

        var result = new HistoryDTO(entries);

        return result.AsMaybe();
    }

    private static Maybe<VotingSessionsDTO> Map(VotingMetadata[] input)
    {
        var dto = input
            .OrderByDescending(x => x.Concluded)
            .Select(x => new VotingSessionDTO(x.VotingSessionId, x.Concluded.ToString("D", new CultureInfo("pl")), x.Concluded.ToString("s")))
            .ToArray();
        var result = new VotingSessionsDTO(dto);

        return result.AsMaybe();
    }

    private static Maybe<VotingState> Map(VotingSessionId? input)
    {
        var state = input == null ? VotingState.Results : VotingState.Voting;
        return state.AsMaybe();
    }

    private async Task<Maybe<VotingSessionId?>> MapAsync((string, DomainUser) input, CancellationToken cancelToken)
    {
        if (string.IsNullOrEmpty(input.Item1))
            return await _votingSessionService.GetCurrentVotingSessionIdAsync(input.Item2.AsMaybe(), cancelToken);

        if (!Guid.TryParse(input.Item1, out var correlationId))
            return new Error<VotingSessionId?>("Invalid id!", ErrorType.IncomingDataIssue);

        return (new VotingSessionId(correlationId) as VotingSessionId?).AsMaybe();
    }
}