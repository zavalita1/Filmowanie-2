using Filmowanie.Database.Entities.Voting;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Notification.Consumers;

public sealed class ConcludeVotingEventConsumer : IConsumer<ConcludeVotingEvent>, IConsumer<Fault<ConcludeVotingEvent>>
{
    private readonly ILogger<ConcludeVotingEventConsumer> _logger;
    private readonly IHubContext<VotingStateHub> _votingHubContext;

    public ConcludeVotingEventConsumer(ILogger<ConcludeVotingEventConsumer> logger, IHubContext<VotingStateHub> votingHubContext)
    {
        _logger = logger;
        _votingHubContext = votingHubContext;
    }

    public Task Consume(ConsumeContext<Fault<ConcludeVotingEvent>> context)
    {
        var message = string.Join(",", context.Message.Exceptions.Select(x => x.Message));
        var callStacks = string.Join(";;;;;;;;;;;;", context.Message.Exceptions.Select(x => x.StackTrace));
        return context.Publish(new ErrorEvent(context.Message.Message.CorrelationId, message, callStacks));
    }

    public async Task Consume(ConsumeContext<ConcludeVotingEvent> context)
    {
        _logger.LogInformation($"Consuming {nameof(ConcludeVotingEvent)}...");
        await _votingHubContext.Clients.All.SendAsync("voting ended", context.CancellationToken);
        _logger.LogInformation($"Consumed {nameof(ConcludeVotingEvent)} event.");
    }
}