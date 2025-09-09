using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Nomination.Consumers;

public sealed class VotingConcludedConsumer : IConsumer<VotingConcludedEvent>, IConsumer<Fault<VotingConcludedEvent>>
{
    private readonly ILogger<VotingConcludedConsumer> _logger;
    private readonly IVotingResultsRepository _repository;
    private readonly IMovieCommandRepository _movieCommandRepository;
    private readonly IGuidProvider _guidProvider;
    private readonly IDateTimeProvider _dateTimeProvider;
    private const int TimeWindow = 10;

    public VotingConcludedConsumer(ILogger<VotingConcludedConsumer> logger, IVotingResultsRepository repository, IMovieCommandRepository movieCommandRepository, IGuidProvider guidProvider, IDateTimeProvider dateTimeProvider)
    {
        _logger = logger;
        _repository = repository;
        _movieCommandRepository = movieCommandRepository;
        _guidProvider = guidProvider;
        _dateTimeProvider = dateTimeProvider;
    }

    public Task Consume(ConsumeContext<Fault<VotingConcludedEvent>> context)
    {
        var message = string.Join(",", context.Message.Exceptions.Select(x => x.Message));
        _logger.LogError($"ERROR WHEN CONSIDERING MOVIES THAT CAN BE NOMINATED AGAIN! {message}.");
        return Task.CompletedTask;
    }

    public async Task Consume(ConsumeContext<VotingConcludedEvent> context)
    {
        _logger.LogInformation($"Consuming {nameof(VotingConcludedEvent)}...");
        
        var message = context.Message;
        var readOnlyVotingResults = await _repository.GetLastNVotingResultsAsync(TimeWindow, context.CancellationToken);
        var lastVotingResult = readOnlyVotingResults.RequireResult().Last();

        var newMoviesToAdd = lastVotingResult.MoviesGoingByeBye.Select(x => GetReadOnlyCanNominateMovieAgainEvent(x, message));
        await _movieCommandRepository.InsertCanBeNominatedAgainAsync(newMoviesToAdd, context.CancellationToken);

        _logger.LogInformation($"Consumed {nameof(VotingConcludedEvent)} event.");
    }

    private IReadOnlyCanNominateMovieAgainEvent GetReadOnlyCanNominateMovieAgainEvent(IReadOnlyEmbeddedMovie x, VotingConcludedEvent message)
    {
        var tenantId = message.Tenant.Id;
        var now = _dateTimeProvider.Now;
        var guid = _guidProvider.NewGuid();
        var id = $"nominate-again-event-{guid}";
        return new CanNominateMovieAgainEventRecord(x, id, now, tenantId);
    }
}