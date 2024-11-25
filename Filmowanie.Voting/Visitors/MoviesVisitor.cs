using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities.Events;
using Filmowanie.Voting.DTOs.Outgoing;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Visitors;

internal interface IGetMoviesForVotingSessionVisitor : IOperationAsyncVisitor<(IVotingStartedEvent, DomainUser), MovieDTO[]>;
internal interface IEnrichMoviesForVotingSessionWithPlaceholdersVisitor : IOperationVisitor<MovieDTO[], MovieDTO[]>;

internal sealed class MoviesVisitor : IGetMoviesForVotingSessionVisitor, IEnrichMoviesForVotingSessionWithPlaceholdersVisitor
{
    private readonly IEventsQueryRepository _eventsQueryRepository;
    private readonly ILogger<MoviesVisitor> _log;

    public MoviesVisitor(IEventsQueryRepository eventsQueryRepository, ILogger<MoviesVisitor> log)
    {
        _eventsQueryRepository = eventsQueryRepository;
        _log = log;
    }

    public OperationResult<MovieDTO[]> Visit(OperationResult<MovieDTO[]> movies)
    {
        throw new NotImplementedException();
    }

    public async Task<OperationResult<MovieDTO[]>> VisitAsync(OperationResult<(IVotingStartedEvent, DomainUser)> input, CancellationToken cancellationToken)
    {
        var resultMovies = new List<MovieDTO>(input.Result!.Item1.Movies.Length);
        var votingSessionId = input.Result.Item1.VotingId;

        var voteAddedEvents = await _eventsQueryRepository.GetVoteAddedEventsAsync(x => 
            x.User.id == input.Result.Item2.Id && x.User.TenantId == input.Result.Item2.Tenant.Id && x.VotingId == votingSessionId, cancellationToken);
        var voteRemovedEvents = await _eventsQueryRepository.GetVoteRemovedEventsAsync(x =>
            x.User.id == input.Result.Item2.Id && x.User.TenantId == input.Result.Item2.Tenant.Id && x.VotingId == votingSessionId, cancellationToken);

        foreach (var movie in input.Result.Item1.Movies)
        {
            var votesToAdd = voteAddedEvents.Where(x => x.Movie.id == movie.id).GroupBy(x => x.VoteType).ToDictionary(x => x.Key, x => x.Count());
            var votesToRemove = voteRemovedEvents.Where(x => x.Movie.id == movie.id).GroupBy(x => x.VoteType).ToDictionary(x => x.Key, x => x.Count());
            var votesToCount = votesToAdd
                .ToDictionary(x => x.Key, x => x.Value - votesToRemove[x.Key])
                .Where(x => x.Value != 0)
                .ToArray();

            if (votesToCount.Length > 1)
            {
                _log.LogError($"Invalid number of added and deleted votes for user: {input.Result.Item2.Id} (tenant: {input.Result.Item2.Tenant})!");
                return new OperationResult<MovieDTO[]>(null, new Error("Invalid number of added and deleted votes!", ErrorType.InvalidState));
            }

            var votes = (int)votesToCount.SingleOrDefault().Key;
            var duration = GetDurationString(movie.DurationInMinutes);
            var movieDto = new MovieDTO(movie.Name, votes, movie.PosterUrl, movie.Description, movie.FilmwebUrl, movie.CreationYear, duration, movie.Genres, movie.Actors,
                movie.Directors, movie.Writers, movie.OriginalTitle);

            resultMovies.Add(movieDto);
        }

        return new OperationResult<MovieDTO[]>(resultMovies.ToArray(), null);
    }

    private static string GetDurationString(int durationInMinutes)
    {
        var duration = TimeSpan.FromMinutes(durationInMinutes);
        var durationHours = (int)duration.TotalHours;
        var durationMinutes = (int)duration.TotalMinutes - 60 * durationHours;
        var result = durationMinutes == 0 ? $"{durationHours} godz." : $"{durationHours} godz. {durationMinutes} min.";
        return result;
    }
}

internal interface IGetCurrentVotingSessionVisitor : IOperationAsyncVisitor<DomainUser, IVotingStartedEvent>;

internal sealed class VotingSessionVisitor : IGetCurrentVotingSessionVisitor
{
    private readonly IEventsQueryRepository _eventsQueryRepository;

    public VotingSessionVisitor(IEventsQueryRepository eventsQueryRepository)
    {
        _eventsQueryRepository = eventsQueryRepository;
    }

    public async Task<OperationResult<IVotingStartedEvent>> VisitAsync(OperationResult<DomainUser> input, CancellationToken cancellationToken)
    {
        var tenantId = input.Result.Tenant.Id;
        var events = await _eventsQueryRepository.GetStartedEventsAsync(x => x.TenantId == tenantId, x => x.Created, 1, cancellationToken);
        var currentVotingStartedEvent = events.Single();
        return new OperationResult<IVotingStartedEvent>(currentVotingStartedEvent, null);
    }
}