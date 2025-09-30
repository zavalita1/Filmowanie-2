using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Services;

// TODO UTs
internal sealed class CurrentVotingService : ICurrentVotingService
{
    private readonly IRequestClient<MoviesListRequestedEvent> getMoviesListRequestClient;
    private readonly IMovieDomainRepository movieQueryRepository;
    private readonly ILogger<CurrentVotingService> log;

    public CurrentVotingService(IRequestClient<MoviesListRequestedEvent> getMoviesListRequestClient, IMovieDomainRepository movieQueryRepository, ILogger<CurrentVotingService> log)
    {
        this.getMoviesListRequestClient = getMoviesListRequestClient;
        this.movieQueryRepository = movieQueryRepository;
        this.log = log;
    }

    public Task<Maybe<IReadOnlyMovieEntity[]>> GetCurrentlyVotedMoviesAsync(Maybe<VotingSessionId> input, CancellationToken cancelToken) => input.AcceptAsync(GetCurrentlyVotedMoviesAsync, log, cancelToken);
    public Task<Maybe<(IReadOnlyEmbeddedMovieWithVotes[], bool IsExtraVoting)>> GetCurrentlyVotedMoviesWithVotesAsync(Maybe<VotingSessionId> input, CancellationToken cancelToken) => input.AcceptAsync(GetCurrentlyVotedMoviesWithVotesAsync, log, cancelToken);

    private async Task<Maybe<(IReadOnlyEmbeddedMovieWithVotes[], bool IsExtraVoting)>> GetCurrentlyVotedMoviesWithVotesAsync(VotingSessionId input, CancellationToken cancelToken)
    {
        try
        {
            var embeddedMovies1 =
                await this.getMoviesListRequestClient.GetResponse<CurrentVotingListResponse>(new MoviesListRequestedEvent(input), cancelToken, TimeSpan.FromSeconds(30));
            var movies = embeddedMovies1.Message.Movies;
            var embeddedMovies = movies.ToArray();
            var result = (embeddedMovies, embeddedMovies1.Message.IsExtraVoting);
            return result.AsMaybe();
        }
        catch (Exception ex)
        {
            this.log.LogError(ex, "Error when trying to get voted movies!");
            return new Error<(IReadOnlyEmbeddedMovieWithVotes[], bool)>(ex.Message, ErrorType.Unknown);
        }
    }

    private async Task<Maybe<IReadOnlyMovieEntity[]>> GetCurrentlyVotedMoviesAsync(VotingSessionId input, CancellationToken cancelToken)
    {
        var embeddedMovies = await GetCurrentlyVotedMoviesWithVotesAsync(input, cancelToken);

        if (embeddedMovies.Error.HasValue)
            return embeddedMovies.Error.Value.ChangeResultType<(IReadOnlyEmbeddedMovieWithVotes[], bool), IReadOnlyMovieEntity[]>();

        var moviesIds = embeddedMovies.Result!.Item1.Select(x => x.Movie.id).ToArray();
        var moviesEntities = await this.movieQueryRepository.GetManyByIdAsync(moviesIds, cancelToken);

        if (moviesEntities.Error.HasValue)
            return moviesEntities.Error.Value;

        if (moviesEntities.Result!.Length != moviesIds.Length)
            return new Error<IReadOnlyMovieEntity[]>("Movies missing in DB!", ErrorType.InvalidState);

        return moviesEntities;
    }
}