using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.DTOs.Outgoing;
using Filmowanie.Voting.Interfaces;
using Filmowanie.Voting.Sagas;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Visitors;

internal sealed class GetVotingResultDTOVisitor : IGetVotingResultDTOVisitor
{
    private readonly IVotingSessionQueryRepository _votingSessionQueryRepository;
    private readonly IRequestClient<MoviesListRequested> _getMoviesListRequestClient;
    private readonly ILogger<GetVotingResultDTOVisitor> _log;

    public GetVotingResultDTOVisitor(IVotingSessionQueryRepository votingSessionQueryRepository, IRequestClient<MoviesListRequested> getMoviesListRequestClient, ILogger<GetVotingResultDTOVisitor> log)
    {
        _votingSessionQueryRepository = votingSessionQueryRepository;
        _getMoviesListRequestClient = getMoviesListRequestClient;
        _log = log;
    }

    public async Task<OperationResult<VotingResultDTO>> VisitAsync(OperationResult<(TenantId Tenant, VotingSessionId? VotingSessionId)> input, CancellationToken cancellationToken)
    {
        var votingResult = await GetReadonlyVotingResultAsync(input, cancellationToken);

        if (votingResult == null)
            return new OperationResult<VotingResultDTO>(null, new Error("No such vote found!", ErrorType.IncomingDataIssue));

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
        return new OperationResult<VotingResultDTO>(result, null);
    }

    private async Task<IReadonlyVotingResult?> GetReadonlyVotingResultAsync(OperationResult<(TenantId Tenant, VotingSessionId? VotingSessionId)> input, CancellationToken cancellationToken)
    {
        IReadonlyVotingResult? votingResult;
        if (input.Result.VotingSessionId == null)
        {
            var currentVoting = await _votingSessionQueryRepository.Get(x => x.Concluded == null, input.Result.Tenant, cancellationToken);

            if (currentVoting == null)
                votingResult = (await _votingSessionQueryRepository.Get(x => x.Concluded != null, input.Result.Tenant, x => x.Concluded!, -1, cancellationToken)).Single();
            else
            {
                // this is for admin only
                var votingSessionId = Guid.Parse(currentVoting.id);
                var embeddedMovies = await _getMoviesListRequestClient.GetResponse<CurrentVotingListResponse>(new MoviesListRequested(votingSessionId), cancellationToken);
                var movies = embeddedMovies.Message.Movies;
                votingResult = new VotingResult(votingSessionId.ToString(), DateTime.Now, 1, DateTime.Now, movies, [], [], [], movies.First().Movie);
            }
        }
        else
        {
            var votingSessionId = input.Result.VotingSessionId!.Value.CorrelationId.ToString();
            var tenantId = input.Result.Tenant;
            votingResult = await _votingSessionQueryRepository.Get(x => x.id == votingSessionId, tenantId, cancellationToken);

        }

        return votingResult;
    }

    private readonly record struct VotingResult(string id, DateTime Created, int TenantId, DateTime? Concluded, IReadOnlyEmbeddedMovieWithVotes[] Movies, IReadOnlyEmbeddedUserWithNominationAward[] UsersAwardedWithNominations, IReadOnlyEmbeddedMovie[] MoviesGoingByeBye, IReadOnlyEmbeddedMovieWithNominationContext[] MoviesAdded, IReadOnlyEmbeddedMovie Winner) : IReadonlyVotingResult;

    public ILogger Log => _log;
}