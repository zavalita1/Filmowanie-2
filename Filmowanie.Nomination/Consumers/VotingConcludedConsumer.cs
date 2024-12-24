using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Nomination.Consumers;

public sealed class VotingConcludedConsumer : IConsumer<VotingConcludedEvent>, IConsumer<Fault<VotingConcludedEvent>>
{
    private readonly ILogger<VotingConcludedConsumer> _logger;
    private readonly IVotingSessionQueryRepository _votesRepository;
    private readonly IMovieCommandRepository _movieCommandRepository;
    private readonly IGuidProvider _guidProvider;
    private readonly IDateTimeProvider _dateTimeProvider;
    private const int TimeWindow = 10;

    public VotingConcludedConsumer(ILogger<VotingConcludedConsumer> logger, IVotingSessionQueryRepository votesRepository, IMovieCommandRepository movieCommandRepository, IGuidProvider guidProvider, IDateTimeProvider dateTimeProvider)
    {
        _logger = logger;
        _votesRepository = votesRepository;
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
        var votingResultOfInterest = (await _votesRepository.Get(x => x.Concluded != null, message.Tenant, x => x.Concluded!, -1 * TimeWindow, context.CancellationToken)).Last();

        var newMoviesToAdd = votingResultOfInterest.MoviesGoingByeBye.Select(x => GetReadOnlyCanNominateMovieAgainEvent(x, message));
        await _movieCommandRepository.InsertCanBeNominatedAgainAsync(newMoviesToAdd, context.CancellationToken);

        _logger.LogInformation($"Consumed {nameof(VotingConcludedEvent)} event.");
    }

    private IReadOnlyCanNominateMovieAgainEvent GetReadOnlyCanNominateMovieAgainEvent(IReadOnlyEmbeddedMovie x, VotingConcludedEvent message) 
        => new CanNominateMovieAgainEventRecord(x,  "nominate-again-event-" + _guidProvider.NewGuid(), _dateTimeProvider.Now, message.Tenant.Id);
}