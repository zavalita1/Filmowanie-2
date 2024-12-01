using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.Deciders;
using Filmowanie.Voting.Deciders.PickUserNomination;
using Filmowanie.Voting.Extensions;
using Filmowanie.Voting.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Consumers;

public sealed class VotingConcludedConsumer : IConsumer<VotingConcludedEvent>
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

    public async Task Consume(ConsumeContext<VotingConcludedEvent> context)
    {
        _logger.LogInformation($"Consuming {nameof(VotingConcludedEvent)}...");
        var now = _dateTimeProvider.Now;
        var message = context.Message;

        var lastVotingResults = (await _votingSessionQueryRepository.Get(x => x.Concluded != null && x.TenantId == message.Tenant.Id, x => x.Concluded!, -1 * DecidersTimeWindow, context.CancellationToken)).ToArray();
        var readonlyCurrentVotingResult = (await _votingSessionQueryRepository.GetCurrent(context.Message.Tenant, context.CancellationToken))!;
        var votingResults = GetVotingResults(readonlyCurrentVotingResult, lastVotingResults.First());

        var moviesAdded = message.NominationsData.Join(message.MoviesWithVotes, x => x.MovieId, x => x.id, (x, y) => new EmbeddedMovieWithNominationContext
        {
            id = x.MovieId,
            Name = y.Name,
            NominatedBy = new EmbeddedUser { id = x.User.Id, Name = x.User.DisplayName, TenantId = message.Tenant.Id },
            NominationConcluded = x.Concluded!.Value,
            NominationStarted = message.VotingStarted
        }).ToArray();

        var assignNominationsUserContexts = GetPickUserToNominateContexts(lastVotingResults, moviesAdded, message);
        var nominations = GetNominations(assignNominationsUserContexts, message, votingResults);

        await _votingSessionCommandRepository.UpdateAsync(readonlyCurrentVotingResult.Id, votingResults.Movies, nominations, now, moviesAdded, votingResults.Winner.Movie, context.CancellationToken);
        
        _logger.LogInformation($"Consumer {nameof(VotingConcludedEvent)}.");
    }

    private VotingResults GetVotingResults(IReadonlyVotingResult current, IReadonlyVotingResult previous)
    {
        var regularDecider = _votingDeciderFactory.ForRegularVoting();
        var moviesWithScores = regularDecider.AssignScores(current.Movies, previous.Movies.Select(x => x.Movie));

        var trashVotingDecider = _votingDeciderFactory.ForTrashVoting();
        var moviesWithTrashScore = trashVotingDecider.AssignScores(current.Movies, previous.Movies.Select(x => x.Movie));

        var moviesGoingByeBye = moviesWithTrashScore.Where(x => x.IsWinner).Select(IReadOnlyEmbeddedMovie (x) => x.Movie.Movie).ToArray();
        var movies = moviesWithScores.Select(x => x.Movie).ToArray();
        var winner = moviesWithScores.Single(x => x.IsWinner).Movie;

        return new (moviesGoingByeBye, movies, winner);
    }

    private static Dictionary<string, PickUserToNominateContext> GetPickUserToNominateContexts(IReadonlyVotingResult[] lastVotingResults, EmbeddedMovieWithNominationContext[] moviesAdded, VotingConcludedEvent message)
    {
        var groups = lastVotingResults
            .SelectMany(x => x.MoviesAdded)
            .Concat(moviesAdded)
            .GroupBy(x => x.NominatedBy.id, x => x.NominationConcluded.Subtract(x.NominationStarted))
            .Select(x => (x.Key, x.ToArray()));

        var missingUsers = message.MoviesWithVotes.SelectMany(x => x.Votes).Select(x => x.User.id).Where(x => groups.All(y => y.Key != x));
        var userContextsPreCalculationMap = groups
            .Concat(missingUsers.Select(y => (y, Array.Empty<TimeSpan>())))
            .ToDictionary(x => x.Item1, x => x.Item2);

        var userVotesMap = message.MoviesWithVotes
            .SelectMany(x => x.Votes.Select(y => new { MovieId = x.id, Vote = y }))
            .GroupBy(x => x.Vote.User.id, x => new { x.MovieId, x.Vote.VoteType})
            .ToDictionary(x => x.Key, x => x.ToArray());

        var contextsDictionary = new Dictionary<string, PickUserToNominateContext>(userContextsPreCalculationMap.Count());

        foreach (var group in userContextsPreCalculationMap)
        {
            var averageNominationPendingTime = group.Value.Length == 0 ? 0 : group.Value.Average(y => y.TotalHours);
            var votingTookPartIn = lastVotingResults.Count(x => x.Movies.SelectMany(y => y.Movie.Votes).Any(y => y.User.id == group.Key));
            var votes = userVotesMap[group.Key].Select(x => (x.MovieId, x.VoteType)).ToArray();

            var entry = new PickUserToNominateContext
            {
                AverageNominationPendingTime = averageNominationPendingTime, 
                NominationsCount = group.Value.Length, 
                ParticipationPercent = (100d / DecidersTimeWindow) * votingTookPartIn,
                Votes = votes
            };
            contextsDictionary[group.Key] = entry;
        }

        return contextsDictionary;
    }

    private List<EmbeddedUserWithNominationAward> GetNominations(Dictionary<string, PickUserToNominateContext> assignNominationsUserContexts, VotingConcludedEvent message, VotingResults votingResults)
    {
        var pickUserNominationStrategy = _pickUserToNominateStrategyFactory.ForRegularVoting();
        var userToNominate = pickUserNominationStrategy.GetUserToNominate(votingResults.Winner.Movie, assignNominationsUserContexts);

        var list = new List<EmbeddedUserWithNominationAward>();
        var winnerAward = new EmbeddedUserWithNominationAward { AwardMessage = "TODO", Decade = votingResults.Winner.Movie.MovieCreationYear.ToDecade(), id = userToNominate.Id, Name = userToNominate.DisplayName, TenantId = message.Tenant.Id };
        list.Add(winnerAward);

        var trashDecider = _pickUserToNominateStrategyFactory.ForTrashVoting();
        foreach (var trashMovie in votingResults.MoviesGoingByeBye)
        {
            var trashNominator = trashDecider.GetUserToNominate(trashMovie, assignNominationsUserContexts);
            var decade = message.MoviesWithVotes.Single(x => x.id == trashMovie.id).MovieCreationYear.ToDecade();
            list.Add(new EmbeddedUserWithNominationAward { AwardMessage = "TODO 22", Decade = decade, id = trashNominator.Id, Name = trashNominator.DisplayName, TenantId = message.Tenant.Id });
        }

        return list;
    }

    private readonly record struct VotingResults(IReadOnlyEmbeddedMovie[] MoviesGoingByeBye, IResultEmbeddedMovie[] Movies, IResultEmbeddedMovie Winner);
}