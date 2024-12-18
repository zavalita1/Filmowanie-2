using Filmowanie.Abstractions;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Visitors;

internal sealed class VotingSessionResultVisitor : IGetVotingSessionsMetadataVisitor
{
    private readonly IVotingSessionQueryRepository _votingSessionQueryRepository;
    private readonly IMovieQueryRepository _movieQueryRepository;
    private readonly ILogger<VotingSessionIdQueryVisitor> _log;

    public VotingSessionResultVisitor(IVotingSessionQueryRepository votingSessionQueryRepository, ILogger<VotingSessionIdQueryVisitor> log, IMovieQueryRepository movieQueryRepository)
    {
        _votingSessionQueryRepository = votingSessionQueryRepository;
        _log = log;
        _movieQueryRepository = movieQueryRepository;
    }

  
    public async Task<OperationResult<VotingMetadata[]>> VisitAsync(OperationResult<TenantId> input, CancellationToken cancellationToken)
    {
        var votingSessions = (await _votingSessionQueryRepository.Get(x => x.TenantId == input.Result.Id && x.Concluded != null, x => new { Id = x.id, x.Concluded, MovieId = x.Winner.id }, cancellationToken)).ToArray();
        var moviesIds = votingSessions.Select(x => x.MovieId).ToArray();
        var movies = await _movieQueryRepository.GetMoviesAsync(x => x.TenantId == input.Result.Id && moviesIds.Contains(x.id), cancellationToken);
        var result = votingSessions
            .Join(movies, x => x.MovieId, x => x.id, (x, y) =>
                new VotingMetadata(x.Id, x.Concluded!.Value, new VotingMetadataWinnerData(((IReadOnlyEntity)y).id, y.Name, y.OriginalTitle, y.CreationYear)))
            .ToArray();

        return new OperationResult<VotingMetadata[]>(result, null);
    }

    public ILogger Log => _log;

}