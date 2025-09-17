using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Nomination.Consumers;

public sealed class ResultsConfirmedConsumer : IConsumer<ResultsConfirmedEvent>, IConsumer<Fault<ResultsConfirmedEvent>>
{
    private readonly ILogger<ResultsConfirmedConsumer> _logger;
    private readonly IRepositoryInUserlessContextProvider _repositoriesProvider;
    private readonly IMovieCommandRepository _movieCommandRepository;
    private readonly IGuidProvider _guidProvider;
    private readonly IDateTimeProvider _dateTimeProvider;
    private const int TimeWindow = 10;

    public ResultsConfirmedConsumer(ILogger<ResultsConfirmedConsumer> logger, IRepositoryInUserlessContextProvider repositoryProvider, IMovieCommandRepository movieCommandRepository, IGuidProvider guidProvider, IDateTimeProvider dateTimeProvider)
    {
        _logger = logger;
        _repositoriesProvider = repositoryProvider;
        _movieCommandRepository = movieCommandRepository;
        _guidProvider = guidProvider;
        _dateTimeProvider = dateTimeProvider;
    }

    public Task Consume(ConsumeContext<Fault<ResultsConfirmedEvent>> context)
    {
        var message = string.Join(",", context.Message.Exceptions.Select(x => x.Message));
        _logger.LogError($"ERROR WHEN CONSIDERING MOVIES THAT CAN BE NOMINATED AGAIN! {message}.");
        return Task.CompletedTask;
    }

    public async Task Consume(ConsumeContext<ResultsConfirmedEvent> context)
    {
        _logger.LogInformation($"Consuming {nameof(ResultsConfirmedEvent)}...");

        try
        {
            var message = context.Message;
            var repo = _repositoriesProvider.GetVotingResultsRepository(message.Tenant);
            var readOnlyVotingResults = await repo.GetLastNVotingResultsAsync(TimeWindow, context.CancellationToken);
            var lastVotingResult = readOnlyVotingResults.RequireResult().Last();

            var newMoviesToAdd = lastVotingResult.MoviesGoingByeBye.Select(x => GetReadOnlyCanNominateMovieAgainEvent(x, message));
            await _movieCommandRepository.InsertCanBeNominatedAgainAsync(newMoviesToAdd, context.CancellationToken);
        }
        catch (Exception ex)
        {
            var msg = "Error occurred during concluding the voting...";
            _logger.LogError(ex, msg);
            var errorEvent = new ErrorEvent(context.Message.VotingSessionId, msg, ex.StackTrace ?? "Unknown");
            await context.Publish(errorEvent);
        }

        _logger.LogInformation($"Consumed {nameof(VotingConcludedEvent)} event.");
    }

    private IReadOnlyCanNominateMovieAgainEvent GetReadOnlyCanNominateMovieAgainEvent(IReadOnlyEmbeddedMovie x, ResultsConfirmedEvent message)
    {
        var tenantId = message.Tenant.Id;
        var now = _dateTimeProvider.Now;
        var guid = _guidProvider.NewGuid();
        var id = $"nominate-again-event-{guid}";
        return new CanNominateMovieAgainEventRecord(x, id, now, tenantId);
    }
}