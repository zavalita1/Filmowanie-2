using System.Runtime.CompilerServices;
using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.DTOs.Outgoing;
using Filmowanie.Voting.Interfaces;
using Filmowanie.Voting.Sagas;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Visitors;

internal sealed class VotingSessionResultIdQueryVisitor : IGetCurrentVotingSessionIdVisitor, IGetCurrentVotingSessionStatusVisitor, IGetVotingSessionResultVisitor, IGetVotingSessionsMetadataVisitor
{
    private readonly IVotingSessionQueryRepository _votingSessionQueryRepository;
    private readonly IRequestClient<MoviesListRequested> _getMoviesListRequestClient;
    private readonly ILogger<VotingSessionResultIdQueryVisitor> _log;

    public VotingSessionResultIdQueryVisitor(IVotingSessionQueryRepository votingSessionQueryRepository, ILogger<VotingSessionResultIdQueryVisitor> log, IRequestClient<MoviesListRequested> getMoviesListRequestClient)
    {
        _votingSessionQueryRepository = votingSessionQueryRepository;
        _log = log;
        _getMoviesListRequestClient = getMoviesListRequestClient;
    }

    // IGetCurrentVotingSessionVisitor
    public async Task<OperationResult<VotingSessionId>> VisitAsync(OperationResult<DomainUser> input, CancellationToken cancellationToken)
    {
        var currentVotingResults = await _votingSessionQueryRepository.Get(x => x.TenantId == input.Result.Tenant.Id && x.Concluded == null, cancellationToken);

        if (currentVotingResults == null)
            return new OperationResult<VotingSessionId>(default, new Error("Voting has not started yet!", ErrorType.InvalidState));

        var correlationId = Guid.Parse(currentVotingResults.Id);
        var votingSessionId = new VotingSessionId(correlationId);
        return new OperationResult<VotingSessionId>(votingSessionId, null);
    }

    // IGetCurrentVotingSessionStatusVisitor
    async Task<OperationResult<VotingState>> IOperationAsyncVisitor<DomainUser, VotingState>.VisitAsync(OperationResult<DomainUser> input, CancellationToken cancellationToken)
    {
        var sagaId = await _votingSessionQueryRepository.Get(x => x.TenantId == input.Result.Tenant.Id && x.Concluded == null, cancellationToken);
        var state = sagaId == null ? VotingState.Results : VotingState.Voting;
        return new OperationResult<VotingState>(state, null);
    }

    public async Task<OperationResult<VotingResultDTO>> VisitAsync(OperationResult<(TenantId Tenant, VotingSessionId? VotingSessionId)> input, CancellationToken cancellationToken)
    {
        var votingResult = await GetReadonlyVotingResultAsync(input, cancellationToken);

        if (votingResult == null)
            return new OperationResult<VotingResultDTO>(null, new Error("No such vote found!", ErrorType.IncomingDataIssue));

        var resultsRows = new List<VotingResultRowDTO>(votingResult.Movies.Length);
        foreach (var movie in votingResult.Movies.OrderByDescending(x => x.VotingScore))
        {
            var isWinner = string.Equals(votingResult.Winner.Name, movie.Movie.Name, StringComparison.OrdinalIgnoreCase);
            var row = new VotingResultRowDTO(movie.Movie.Name, movie.VotingScore, isWinner);
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

        var result = new VotingResultDTO(resultsRows.ToArray(), trashRows.ToArray());
        return new OperationResult<VotingResultDTO>(result, null);
    }

    private async Task<IReadonlyVotingResult?> GetReadonlyVotingResultAsync(OperationResult<(TenantId Tenant, VotingSessionId? VotingSessionId)> input, CancellationToken cancellationToken)
    {
        IReadonlyVotingResult? votingResult;
        if (input.Result.VotingSessionId == null)
        {
            var currentVoting = await _votingSessionQueryRepository.Get(x => x.TenantId == input.Result.Tenant.Id && x.Concluded == null, cancellationToken);

            if (currentVoting == null)
                votingResult = (await _votingSessionQueryRepository.Get(x => x.TenantId == input.Result.Tenant.Id && x.Concluded != null, x => x.Concluded!, -1, cancellationToken)).Single();
            else
            {
                // this is for admin only
                var votingSessionId = Guid.Parse(currentVoting.Id);
                var embeddedMovies = await _getMoviesListRequestClient.GetResponse<CurrentVotingListResponse>(new MoviesListRequested(votingSessionId), cancellationToken);
                var movies = embeddedMovies.Message.Movies;
                votingResult = new VotingResult(votingSessionId.ToString(), DateTime.Now, 1, DateTime.Now, movies, [], [], [], movies.First().Movie);
            }
        }
        else
            votingResult = await _votingSessionQueryRepository.Get(x => x.TenantId == input.Result.Tenant.Id && x.Id == input.Result.VotingSessionId!.Value.CorrelationId.ToString(),
                cancellationToken);

        return votingResult;
    }

    public async Task<OperationResult<VotingMetadata[]>> VisitAsync(OperationResult<TenantId> input, CancellationToken cancellationToken)
    {
        var votingSessions = await _votingSessionQueryRepository.Get(x => x.TenantId == input.Result.Id && x.Concluded != null, x => new { x.Id, x.Concluded, x.Winner.Name }, cancellationToken);
        var result = votingSessions.Select(x => new VotingMetadata(x.Id, x.Concluded!.Value, x.Name)).ToArray();

        return new OperationResult<VotingMetadata[]>(result, null);
    }

    public ILogger Log => _log;

    private readonly record struct VotingResult(string Id, DateTime Created, int TenantId, DateTime? Concluded, IReadOnlyEmbeddedMovieWithVotes[] Movies, IReadOnlyEmbeddedUserWithNominationAward[] UsersAwardedWithNominations, IReadOnlyEmbeddedMovie[] MoviesGoingByeBye, IReadOnlyEmbeddedMovieWithNominationContext[] MoviesAdded, IReadOnlyEmbeddedMovie Winner) : IReadonlyVotingResult;
}