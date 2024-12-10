using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Extensions;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.Deciders;
using Filmowanie.Voting.Deciders.PickUserNomination;
using Filmowanie.Voting.Helpers;
using Filmowanie.Voting.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Consumers;

public sealed class VotingConcludedConsumer : IConsumer<VotingConcludedEvent>, IConsumer<Fault<VotingConcludedEvent>>
{
    private readonly ILogger<VotingConcludedConsumer> _logger;
    private readonly IVotingSessionCommandRepository _votingSessionCommandRepository;
    private readonly IVotingSessionQueryRepository _votingSessionQueryRepository;
    private readonly IVotingDeciderFactory _votingDeciderFactory;
    private readonly IPickUserToNominateStrategyFactory _pickUserToNominateStrategyFactory;
    private readonly IDateTimeProvider _dateTimeProvider;

    private const int DecidersTimeWindow = 10;

    public VotingConcludedConsumer(ILogger<VotingConcludedConsumer> logger, IVotingSessionCommandRepository votingSessionCommandRepository, IDateTimeProvider dateTimeProvider, IVotingSessionQueryRepository votingSessionQueryRepository, IVotingDeciderFactory votingDeciderFactory, IPickUserToNominateStrategyFactory pickUserToNominateStrategyFactory)
    {
        _logger = logger;
        _votingSessionCommandRepository = votingSessionCommandRepository;
        _dateTimeProvider = dateTimeProvider;
        _votingSessionQueryRepository = votingSessionQueryRepository;
        _votingDeciderFactory = votingDeciderFactory;
        _pickUserToNominateStrategyFactory = pickUserToNominateStrategyFactory;
    }

    public Task Consume(ConsumeContext<Fault<VotingConcludedEvent>> context)
    {
        var message = string.Join(",", context.Message.Exceptions.Select(x => x.Message));
        var callStacks = string.Join(";;;;;;;;;;;;", context.Message.Exceptions.Select(x => x.StackTrace));
        return context.Publish(new ErrorEvent(context.Message.Message.CorrelationId, message, callStacks));
    }

    public async Task Consume(ConsumeContext<VotingConcludedEvent> context)
    {
        _logger.LogInformation($"Consuming {nameof(VotingConcludedEvent)}...");
        var now = _dateTimeProvider.Now;
        var message = context.Message;

        var lastVotingResults = (await _votingSessionQueryRepository.Get(x => x.Concluded != null && x.TenantId == message.Tenant.Id, x => x.Concluded!, -1 * DecidersTimeWindow, context.CancellationToken)).ToArray();
        var readonlyCurrentVotingResult = (await _votingSessionQueryRepository.Get(x => x.TenantId == message.Tenant.Id && x.Concluded == null, context.CancellationToken))!;
        var votingResults = GetVotingResults(message.MoviesWithVotes, lastVotingResults.FirstOrDefault()); // null only if it's initial voting

        var moviesAdded = message.NominationsData.Join(message.MoviesWithVotes, x => x.MovieId, x => x.Movie.id, (x, y) => new EmbeddedMovieWithNominationContext
        {
            Movie = y.Movie.AsMutable(),
            NominatedBy = new EmbeddedUser { id = x.User.Id, Name = x.User.DisplayName, TenantId = message.Tenant.Id },
            NominationConcluded = x.Concluded!.Value,
            NominationStarted = message.VotingStarted
        }).ToArray();

        var assignNominationsUserContexts = GetPickUserToNominateContexts(lastVotingResults, moviesAdded, message);
        var nominations = GetNominations(assignNominationsUserContexts, message, votingResults);

        await _votingSessionCommandRepository.UpdateAsync(readonlyCurrentVotingResult.id, votingResults.Movies, nominations, now, moviesAdded, votingResults.Winner, context.CancellationToken);
        await context.Publish(new ResultsCalculated(message.CorrelationId));

        _logger.LogInformation($"Consumed {nameof(VotingConcludedEvent)} event.");
    }

    private VotingResults GetVotingResults(IReadOnlyEmbeddedMovieWithVotes[] currentMovies, IReadonlyVotingResult? previous)
    {
        var regularDecider = _votingDeciderFactory.ForRegularVoting();
        var previousMovies = previous?.Movies.ToArray() ?? [];
        var moviesWithScores = regularDecider.AssignScores(currentMovies, previousMovies).ToArray();

        var trashVotingDecider = _votingDeciderFactory.ForTrashVoting();
        var moviesWithTrashScore = trashVotingDecider.AssignScores(currentMovies, previousMovies);

        var moviesGoingByeBye = moviesWithTrashScore.Where(x => x.IsWinner).Select(IReadOnlyEmbeddedMovie (x) => x.Movie.Movie).ToArray();
        var winner = moviesWithScores.Single(x => x.IsWinner).Movie;
        var movies = moviesWithScores.Select(x => x.Movie).OrderByDescending(x => x.VotingScore).ThenByDescending(x => x.Movie == winner.Movie ? 1 : 0).ToArray();

        return new (moviesGoingByeBye, movies, winner.Movie);
    }

    private static Dictionary<IReadOnlyEmbeddedUser, PickUserToNominateContext> GetPickUserToNominateContexts(IReadonlyVotingResult[] lastVotingResults, EmbeddedMovieWithNominationContext[] moviesAdded, VotingConcludedEvent message)
    {
        var groups = lastVotingResults
            .SelectMany(x => x.MoviesAdded)
            .Concat(moviesAdded)
            .GroupBy(x => x.NominatedBy, x => x.NominationConcluded.Subtract(x.NominationStarted), ReadOnlyEmbeddedUserEqualityComparer.Instance)
            .Select(x => (x.Key, x.ToArray()));

        var missingUsers = message.MoviesWithVotes.SelectMany(x => x.Votes).Select(x => x.User).Where(x => groups.All(y => y.Key.id != x.id));
        var userContextsPreCalculationMap = groups
            .Concat(missingUsers.Select(y => (y, Array.Empty<TimeSpan>())))
            .ToDictionary(x => x.Item1, x => x.Item2, ReadOnlyEmbeddedUserEqualityComparer.Instance);

        var userVotesMap = message.MoviesWithVotes
            .SelectMany(x => x.Votes.Select(y => new { MovieId = x.Movie.id, Vote = y }))
            .GroupBy(x => x.Vote.User, x => new { x.MovieId, x.Vote.VoteType}, ReadOnlyEmbeddedUserEqualityComparer.Instance)
            .ToDictionary(x => x.Key, x => x.ToArray(), ReadOnlyEmbeddedUserEqualityComparer.Instance);

        var contextsDictionary = new Dictionary<IReadOnlyEmbeddedUser, PickUserToNominateContext>(userContextsPreCalculationMap.Count());

        foreach (var group in userContextsPreCalculationMap)
        {
            var averageNominationPendingTime = group.Value.Length == 0 ? 0 : group.Value.Average(y => y.TotalDays);
            var votingTookPartIn = 1 + lastVotingResults.Count(x => x.Movies.SelectMany(y => y.Votes).Any(y => y.User.id == group.Key.id));
            var votes = userVotesMap[group.Key].Select(x => (x.MovieId, x.VoteType)).ToArray();

            var entry = new PickUserToNominateContext
            {
                AverageNominationPendingTimeInDays = averageNominationPendingTime, 
                NominationsCount = group.Value.Length,
                ParticipationPercent = (100d / (lastVotingResults.Length + 1)) * votingTookPartIn,
                Votes = votes
            };
            contextsDictionary[group.Key] = entry;
        }

        return contextsDictionary;
    }

    private List<EmbeddedUserWithNominationAward> GetNominations(Dictionary<IReadOnlyEmbeddedUser, PickUserToNominateContext> assignNominationsUserContexts, VotingConcludedEvent message, VotingResults votingResults)
    {
        var pickUserNominationStrategy = _pickUserToNominateStrategyFactory.ForRegularVoting();
        var userToNominate = pickUserNominationStrategy.GetUserToNominate(votingResults.Winner, assignNominationsUserContexts);

        var list = new List<EmbeddedUserWithNominationAward>();
        var winnerAward = new EmbeddedUserWithNominationAward { AwardMessage = "TODO", Decade = votingResults.Winner.MovieCreationYear.ToDecade(), User = userToNominate.AsMutable() };
        list.Add(winnerAward);

        var trashDecider = _pickUserToNominateStrategyFactory.ForTrashVoting();
        foreach (var trashMovie in votingResults.MoviesGoingByeBye)
        {
            var trashNominator = trashDecider.GetUserToNominate(trashMovie, assignNominationsUserContexts);
            var decade = message.MoviesWithVotes.Single(x => x.Movie.id == trashMovie.id).Movie.MovieCreationYear.ToDecade();
            list.Add(new EmbeddedUserWithNominationAward { AwardMessage = "TODO 22", Decade = decade, User = trashNominator.AsMutable() });
        }

        return list;
    }

    private readonly record struct VotingResults(IReadOnlyEmbeddedMovie[] MoviesGoingByeBye, IReadOnlyEmbeddedMovieWithVotes[] Movies, IReadOnlyEmbeddedMovie Winner);
}