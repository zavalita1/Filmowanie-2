using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.DTOs.Outgoing;
using Filmowanie.Voting.Interfaces;
using Microsoft.Extensions.Logging;
using DomainUser = Filmowanie.Abstractions.DomainModels.DomainUser;
using TenantId = Filmowanie.Abstractions.DomainModels.TenantId;

namespace Filmowanie.Voting.Services;

// TODO UTs
internal sealed class MovieVotingResultService : IMovieVotingResultService
{
    private readonly ICurrentVotingService currentVotingService;
    private readonly IMovieDomainRepository movieQueryRepository;
    private readonly IVotingResultsRepository votingResultsRepository;
    private readonly ILogger<MovieVotingResultService> log;

    public MovieVotingResultService(IMovieDomainRepository movieQueryRepository, ILogger<MovieVotingResultService> log, IVotingResultsRepository votingResultsRepository, ICurrentVotingService currentVotingService)
    {
        this.movieQueryRepository = movieQueryRepository;
        this.log = log;
        this.votingResultsRepository = votingResultsRepository;
        this.currentVotingService = currentVotingService;
    }
   
    public Task<Maybe<VotingResultDTO>> GetVotingResultsAsync(Maybe<DomainUser> maybeCurrentUser, Maybe<VotingSessionId> maybeVotingId, CancellationToken cancelToken) =>
        maybeCurrentUser.Merge(maybeVotingId).AcceptAsync(GetVotingResultsAsync, this.log, cancelToken);

    public Task<Maybe<VotingMetadata[]>> GetVotingMetadataAsync(Maybe<TenantId> input, CancellationToken cancelToken) =>
        input.AcceptAsync(GetVotingMetadata, this.log, cancelToken);

    private async Task<Maybe<VotingMetadata[]>> GetVotingMetadata(TenantId input, CancellationToken cancelToken)
    {
        var maybeVotingSessions = await this.votingResultsRepository.GetAllVotingResultsMetadataAsync(cancelToken);

        if (maybeVotingSessions.Error.HasValue)
            return maybeVotingSessions.Error.Value.ChangeResultType<IEnumerable<IReadOnlyVotingResultMetadata>, VotingMetadata[]>();

        var votingSessions = maybeVotingSessions.RequireResult().ToArray();
        var moviesIds = votingSessions.Select(x => x.WinnerMovieId.Id).ToArray();
        var movies = await this.movieQueryRepository.GetManyByIdAsync(moviesIds, cancelToken);

        if (movies.Error.HasValue)
            return movies.Error.Value.ChangeResultType<IReadOnlyMovieEntity[], VotingMetadata[]>();

        var result = votingSessions
            .Join(movies.Result!, x => x.WinnerMovieId.Id, x => x.id, GetVotingMetadata)
            .ToArray();

        return new Maybe<VotingMetadata[]>(result, null);
    }

    private static VotingMetadata GetVotingMetadata(IReadOnlyVotingResultMetadata x, IReadOnlyMovieEntity y)
    {
        var votingSessionId = x.VotingResultId.CorrelationId.ToString();
        var votingMetadataWinnerData = new VotingMetadataWinnerData(y.id, y.Name, y.OriginalTitle, y.CreationYear, y.FilmwebUrl);
        return new VotingMetadata(votingSessionId, x.Concluded, votingMetadataWinnerData);
    }

    private async Task<Maybe<VotingResultDTO>> GetVotingResultsAsync((DomainUser CurrentUser, Abstractions.DomainModels.VotingSessionId VotingSessionId) input, CancellationToken cancelToken)
    {
        var votingResult = await GetReadonlyVotingResultAsync(input, cancelToken);

        if (votingResult.Error.HasValue)
            return votingResult.Error.Value.ChangeResultType<IReadOnlyVotingResult, VotingResultDTO>();

        var resultsRows = new List<VotingResultRowDTO>(votingResult.Result!.Movies.Length);
        var sortedMovies = votingResult.Result!.Movies.OrderByDescending(x => x.VotingScore).ThenByDescending(x => x.Movie.id == votingResult.Result!.Winner!.Movie.id ? 1 : 0).ToArray();
        
        for (var i = 0; i < sortedMovies.Length; i++)
        {
            var movie = sortedMovies[i];
            var row = new VotingResultRowDTO(movie.Movie.Name, movie.VotingScore, i == 0);
            resultsRows.Add(row);
        }

        var trashRows = new List<TrashVotingResultRowDTO>(votingResult.Result!.Movies.Length);
        foreach (var movie in votingResult.Result!.Movies)
        {
            var voters = movie.Votes.Where(x => x.VoteType == VoteType.Thrash).Select(x => x.User.Name).ToArray();
            var isAwarded = votingResult.Result!.MoviesGoingByeBye.Any(x => string.Equals(x.Name, movie.Movie.Name, StringComparison.OrdinalIgnoreCase));
            var row = new TrashVotingResultRowDTO(movie.Movie.Name, voters, isAwarded);
            trashRows.Add(row);
        }

        var sortedTrash = trashRows.OrderByDescending(x => x.IsAwarded ? 1 : 0).ThenByDescending(x => x.Voters.Length).ToArray();
        var result = new VotingResultDTO(resultsRows.ToArray(), sortedTrash);
        return new Maybe<VotingResultDTO>(result, null);
    }

    private async Task<Maybe<IReadOnlyVotingResult>> GetReadonlyVotingResultAsync((DomainUser CurrentUser, Abstractions.DomainModels.VotingSessionId? VotingSessionId) input, CancellationToken cancelToken)
    {
        var result = await this.votingResultsRepository.GetByIdAsync(input.VotingSessionId!.Value, cancelToken);

        if (result.Result == null)
            return new Error<IReadOnlyVotingResult>("No such vote found!", ErrorType.IncomingDataIssue);

        if (result.Result.Concluded == null)
        {
            if (!input.CurrentUser.IsAdmin)
                return new Error<IReadOnlyVotingResult>("Only admin can view current voting's results!", ErrorType.AuthorizationIssue);

            // for admin only
            var movies = await this.currentVotingService.GetCurrentlyVotedMoviesWithVotesAsync(input.VotingSessionId!.Value.AsMaybe(), cancelToken);

            if (movies.Error.HasValue)
                return movies.Error.Value.ChangeResultType<IReadOnlyEmbeddedMovieWithVotes[], IReadOnlyVotingResult>();

            var readOnlyEmbeddedMovie = movies.Result!.First().Movie;
            var readOnlyEmbeddedMovieWithNominatedBy = new EmbeddedMovieWithNominationContext(readOnlyEmbeddedMovie);
            IReadOnlyVotingResult votingResult = new VotingResult(input.VotingSessionId!.Value.CorrelationId.ToString(), DateTime.Now, 1, DateTime.Now, movies.Result, [], [], [], readOnlyEmbeddedMovieWithNominatedBy);
            return votingResult.AsMaybe();
        }

        return result.Result!.AsMaybe();
    }

    private readonly record struct VotingResult(string id, DateTime Created, int TenantId, DateTime? Concluded, IReadOnlyEmbeddedMovieWithVotes[] Movies, IReadOnlyEmbeddedUserWithNominationAward[] UsersAwardedWithNominations, IReadOnlyEmbeddedMovie[] MoviesGoingByeBye, IReadOnlyEmbeddedMovieWithNominationContext[] MoviesAdded, IReadOnlyEmbeddedMovieWithNominatedBy Winner) : IReadOnlyVotingResult;
}
