using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Database.Repositories;
using Filmowanie.Voting.Interfaces;
using Filmowanie.Voting.Sagas;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Services;

internal sealed class CurrentVotingService : ICurrentVotingService
{
    private readonly IRequestClient<MoviesListRequestedEvent> _getMoviesListRequestClient;
    private readonly IMovieDomainRepository _movieQueryRepository;
    private readonly ILogger<CurrentVotingService> _log;

    public CurrentVotingService(IRequestClient<MoviesListRequestedEvent> getMoviesListRequestClient, IMovieDomainRepository movieQueryRepository, ILogger<CurrentVotingService> log)
    {
        _getMoviesListRequestClient = getMoviesListRequestClient;
        _movieQueryRepository = movieQueryRepository;
        _log = log;
    }

    public Task<Maybe<IReadOnlyMovieEntity[]>> GetCurrentlyVotedMoviesAsync(Maybe<VotingSessionId> input, CancellationToken cancelToken) => input.AcceptAsync(GetCurrentlyVotedMoviesAsync, _log, cancelToken);
    public Task<Maybe<IReadOnlyEmbeddedMovieWithVotes[]>> GetCurrentlyVotedMoviesWithVotesAsync(Maybe<VotingSessionId> input, CancellationToken cancelToken) => input.AcceptAsync(GetCurrentlyVotedMoviesWithVotesAsync, _log, cancelToken);

    private async Task<Maybe<IReadOnlyEmbeddedMovieWithVotes[]>> GetCurrentlyVotedMoviesWithVotesAsync(VotingSessionId input, CancellationToken cancelToken)
    {
        try
        {
            var embeddedMovies1 =
                await _getMoviesListRequestClient.GetResponse<CurrentVotingListResponse>(new MoviesListRequestedEvent(input), cancelToken, TimeSpan.FromSeconds(30));
            var movies = embeddedMovies1.Message.Movies;
            var embeddedMovies = movies.ToArray();
            return embeddedMovies.AsMaybe();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error when trying to get voted movies!");
            return new Error<IReadOnlyEmbeddedMovieWithVotes[]>(ex.Message, ErrorType.Unknown);
        }
    }

    private async Task<Maybe<IReadOnlyMovieEntity[]>> GetCurrentlyVotedMoviesAsync(VotingSessionId input, CancellationToken cancelToken)
    {
        var embeddedMovies = await GetCurrentlyVotedMoviesWithVotesAsync(input, cancelToken);

        if (embeddedMovies.Error.HasValue)
            return embeddedMovies.Error.Value.ChangeResultType<IReadOnlyEmbeddedMovieWithVotes[], IReadOnlyMovieEntity[]>();

        var moviesIds = embeddedMovies.Result!.Select(x => x.Movie.id).ToArray();
        var moviesEntities = await _movieQueryRepository.GetManyByIdAsync(moviesIds, cancelToken);

        if (moviesEntities.Error.HasValue)
            return moviesEntities.Error.Value;

        if (moviesEntities.Result!.Length != moviesIds.Length)
            return new Error<IReadOnlyMovieEntity[]>("Movies missing in DB!", ErrorType.InvalidState);

        return moviesEntities;
    }
}