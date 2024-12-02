using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Helpers;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Interfaces;
using Filmowanie.Nomination.DTOs.Outgoing;
using Filmowanie.Nomination.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Nomination.Visitors;

internal sealed class NominationsVisitor : IGetNominationsVisitor
{
    private readonly IRequestClient<NominationsRequested> _getNominationsRequestClient;
    private readonly IMovieQueryRepository _movieQueryRepository;
    private readonly ILogger<NominationsVisitor> _log;

    public NominationsVisitor(IRequestClient<NominationsRequested> getNominationsRequestClient, IMovieQueryRepository movieQueryRepository, ILogger<NominationsVisitor> log)
    {
        _getNominationsRequestClient = getNominationsRequestClient;
        _movieQueryRepository = movieQueryRepository;
        _log = log;
    }

    public async Task<OperationResult<NominationsDataDTO>> VisitAsync(OperationResult<(VotingSessionId, DomainUser)> input, CancellationToken cancellationToken)
    {
        var nominationsRequested = new NominationsRequested(input.Result.Item1.CorrelationId);
        var nominations = await _getNominationsRequestClient.GetResponse<CurrentNominationsResponse>(nominationsRequested, cancellationToken);

        var user = input.Result.Item2;
        var nominationDecades = nominations.Message.Nominations.Where(x => x.User.Id == user.Id).Select(x => x.Year.ToString()[1..]).ToArray();
        var moviesThatCanBeNominatedAgainList = await _movieQueryRepository.GetMoviesThatCanBeNominatedAgainEntityAsync(x => x.TenantId == user.Tenant.Id, cancellationToken);
        var moviesThatCanBeNominatedAgainIds = moviesThatCanBeNominatedAgainList?.Movies.Select(x => x.id).ToArray() ?? [];
        var moviesThatCanBeNominatedAgain = await _movieQueryRepository.GetMoviesAsync(x => moviesThatCanBeNominatedAgainIds.Contains(x.id), cancellationToken);

        var moviesThatCanBeNominatedAgainDTOs = moviesThatCanBeNominatedAgain.Select(x => new MovieDTO(x.id, x.Name, x.PosterUrl, x.Description, x.FilmwebUrl, x.CreationYear,
            StringHelper.GetDurationString(x.DurationInMinutes), x.Genres, x.Actors, x.Directors, x.Writers, x.OriginalTitle)).ToArray();

        var result = new NominationsDataDTO { Nominations = nominationDecades, MoviesThatCanBeNominatedAgain = moviesThatCanBeNominatedAgainDTOs };

        return new OperationResult<NominationsDataDTO>(result, null);
    }

    public ILogger Log => _log;
}