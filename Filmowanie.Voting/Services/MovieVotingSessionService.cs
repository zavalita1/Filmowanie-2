using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Helpers;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.DTOs.Outgoing;
using Filmowanie.Voting.Interfaces;
using Filmowanie.Voting.Sagas;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Services;

internal sealed class MovieVotingSessionService : IMovieVotingSessionService
{
    private readonly IRequestClient<MoviesListRequestedEvent> _getMoviesListRequestClient;
    private readonly IMovieQueryRepository _movieQueryRepository;
    private readonly IVotingSessionQueryRepository _votingSessionQueryRepository;
    private readonly ILogger<MovieVotingSessionService> _log;

    public MovieVotingSessionService(IRequestClient<MoviesListRequestedEvent> getMoviesListRequestClient, IMovieQueryRepository movieQueryRepository, ILogger<MovieVotingSessionService> log, IVotingSessionQueryRepository votingSessionQueryRepository)
    {
        _getMoviesListRequestClient = getMoviesListRequestClient;
        _movieQueryRepository = movieQueryRepository;
        _log = log;
        _votingSessionQueryRepository = votingSessionQueryRepository;
    }

    public Task<Maybe<MovieDTO[]>> GetCurrentlyVotedMoviesAsync(Maybe<(VotingSessionId, DomainUser)> input, CancellationToken cancellationToken) => input.AcceptAsync(GetCurrentlyVotedMovies, _log, cancellationToken);

    public Task<Maybe<VotingResultDTO>> GetVotingResultsAsync(Maybe<(TenantId Tenant, VotingSessionId? VotingSessionId)> input, CancellationToken cancellationToken) =>
        input.AcceptAsync(GetVotingResultsAsync, _log, cancellationToken);

    public Task<Maybe<VotingMetadata[]>> GetVotingMetadataAsync(Maybe<TenantId> input, CancellationToken cancellationToken) =>
        input.AcceptAsync(GetVotingMetadata, _log, cancellationToken);

    private async Task<Maybe<VotingMetadata[]>> GetVotingMetadata(TenantId input, CancellationToken cancellationToken)
    {
        var votingSessions = (await _votingSessionQueryRepository.Get(x => x.Concluded != null, x => new { Id = x.id, x.Concluded, MovieId = x.Winner.id }, input, cancellationToken)).ToArray();
        var moviesIds = votingSessions.Select(x => x.MovieId).ToArray();
        var movies = await _movieQueryRepository.GetMoviesAsync(x => moviesIds.Contains(x.id), input, cancellationToken);
        var result = votingSessions
            .Join(movies, x => x.MovieId, x => x.id, (x, y) =>
                new VotingMetadata(x.Id, x.Concluded!.Value, new VotingMetadataWinnerData(((IReadOnlyEntity)y).id, y.Name, y.OriginalTitle, y.CreationYear)))
            .ToArray();

        return new Maybe<VotingMetadata[]>(result, null);
    }


    private async Task<Maybe<VotingResultDTO>> GetVotingResultsAsync((TenantId Tenant, VotingSessionId? VotingSessionId) input, CancellationToken cancellationToken)
    {
        var votingResult = await GetReadonlyVotingResultAsync(input, cancellationToken);

        if (votingResult == null)
            return new Error("No such vote found!", ErrorType.IncomingDataIssue).AsMaybe<VotingResultDTO>();

        var resultsRows = new List<VotingResultRowDTO>(votingResult.Movies.Length);
        var sortedMovies = votingResult.Movies.OrderByDescending(x => x.VotingScore).ThenByDescending(x => x.Movie.id == votingResult.Winner.id ? 1 : 0).ToArray();
        
        for (var i = 0; i < sortedMovies.Length; i++)
        {
            var movie = sortedMovies[i];
            var row = new VotingResultRowDTO(movie.Movie.Name, movie.VotingScore, i == 0);
            resultsRows.Add(row);
        }

        var trashRows = new List<TrashVotingResultRowDTO>(votingResult.Movies.Length);
        foreach (var movie in votingResult.Movies)
        {
            var voters = movie.Votes.Where(x => x.VoteType == VoteType.Thrash).Select(x => x.User.Name).ToArray();
            var isAwarded = votingResult.MoviesGoingByeBye.Any(x => string.Equals(x.Name, movie.Movie.Name, StringComparison.OrdinalIgnoreCase));
            var row = new TrashVotingResultRowDTO(movie.Movie.Name, voters, isAwarded);
            trashRows.Add(row);
        }

        var sortedTrash = trashRows.OrderByDescending(x => x.IsAwarded ? 1 : 0).ThenByDescending(x => x.Voters.Length).ToArray();
        var result = new VotingResultDTO(resultsRows.ToArray(), sortedTrash);
        return new Maybe<VotingResultDTO>(result, null);
    }

    private async Task<IReadOnlyVotingResult?> GetReadonlyVotingResultAsync((TenantId Tenant, VotingSessionId? VotingSessionId) input, CancellationToken cancellationToken)
    {
        IReadOnlyVotingResult? votingResult;
        if (input.VotingSessionId == null)
        {
            var currentVoting = await _votingSessionQueryRepository.Get(x => x.Concluded == null, input.Tenant, cancellationToken);

            if (currentVoting == null)
                votingResult = (await _votingSessionQueryRepository.Get(x => x.Concluded != null, input.Tenant, x => x.Concluded!, -1, cancellationToken)).Single();
            else
            {
                // this is for admin only
                var votingSessionId = new VotingSessionId(Guid.Parse(currentVoting.id));
                var embeddedMovies = await _getMoviesListRequestClient.GetResponse<CurrentVotingListResponse>(new MoviesListRequestedEvent(votingSessionId), cancellationToken);
                var movies = embeddedMovies.Message.Movies;
                votingResult = new VotingResult(votingSessionId.ToString(), DateTime.Now, 1, DateTime.Now, movies, [], [], [], movies.First().Movie);
            }
        }
        else
        {
            var votingSessionId = input.VotingSessionId!.Value.CorrelationId.ToString();
            var tenantId = input.Tenant;
            votingResult = await _votingSessionQueryRepository.Get(x => x.id == votingSessionId, tenantId, cancellationToken);

        }

        return votingResult;
    }

    private readonly record struct VotingResult(string id, DateTime Created, int TenantId, DateTime? Concluded, IReadOnlyEmbeddedMovieWithVotes[] Movies, IReadOnlyEmbeddedUserWithNominationAward[] UsersAwardedWithNominations, IReadOnlyEmbeddedMovie[] MoviesGoingByeBye, IReadOnlyEmbeddedMovieWithNominationContext[] MoviesAdded, IReadOnlyEmbeddedMovie Winner) : IReadOnlyVotingResult;

    private async Task<Maybe<MovieDTO[]>> GetCurrentlyVotedMovies((VotingSessionId, DomainUser) input, CancellationToken cancellationToken)
    {
        var @event = new MoviesListRequestedEvent(input.Item1);
        var embeddedMovies = await _getMoviesListRequestClient.GetResponse<CurrentVotingListResponse>(@event, cancellationToken);
        var moviesIds = embeddedMovies.Message.Movies.Select(x => x.Movie.id).ToArray();
        var moviesEntities = await _movieQueryRepository.GetMoviesAsync(x => moviesIds.Contains(x.id), input.Item2.Tenant, cancellationToken);

        if (moviesEntities.Length != moviesIds.Length)
            return new Error("Movies missing in DB!", ErrorType.InvalidState).AsMaybe<MovieDTO[]>();

        var movies = embeddedMovies.Message.Movies.Join(moviesEntities, x => x.Movie.id, x => x.id, (x, y) => new { Movie = y, x.Votes });

        var resultMovies = new List<MovieDTO>(embeddedMovies.Message.Movies.Length);

        foreach (var movie in movies)
        {
            _log.LogInformation($"Mapping movie: {movie.Movie.Name}");
            var votes = (int?)movie.Votes.SingleOrDefault(x => x.User.id == input.Item2.Id)?.VoteType ?? 0;
            var duration = StringHelper.GetDurationString(movie.Movie.DurationInMinutes);
            var movieDto = new MovieDTO(movie.Movie.id, movie.Movie.Name, votes, movie.Movie.PosterUrl, movie.Movie.Description, movie.Movie.FilmwebUrl, movie.Movie.CreationYear, duration, movie.Movie.Genres, movie.Movie.Actors,
                movie.Movie.Directors, movie.Movie.Writers, movie.Movie.OriginalTitle);

            resultMovies.Add(movieDto);
        }

        return resultMovies.ToArray().AsMaybe();
    }
}
