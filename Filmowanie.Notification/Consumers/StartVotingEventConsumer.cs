using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Notification.Services;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Notification.Consumers;

public sealed class StartVotingEventConsumer : IConsumer<StartVotingEvent>, IConsumer<Fault<StartVotingEvent>>
{
    private readonly ILogger<StartVotingEventConsumer> _logger;
    private readonly IHubContext<VotingStateHub> _votingHubContext;

    public StartVotingEventConsumer(ILogger<StartVotingEventConsumer> logger, IHubContext<VotingStateHub> votingHubContext)
    {
        _logger = logger;
        _votingHubContext = votingHubContext;
    }

    public Task Consume(ConsumeContext<Fault<StartVotingEvent>> context)
    {
        var message = string.Join(",", context.Message.Exceptions.Select(x => x.Message));
        var callStacks = string.Join(";;;;;;;;;;;;", context.Message.Exceptions.Select(x => x.StackTrace));
        return context.Publish(new ErrorEvent(context.Message.Message.VotingSessionId, message, callStacks));
    }

    public async Task Consume(ConsumeContext<StartVotingEvent> context)
    {
        _logger.LogInformation($"Consuming {nameof(StartVotingEvent)}...");
        await _votingHubContext.Clients.All.SendAsync("voting started", context.CancellationToken);
        _logger.LogInformation($"Consumed {nameof(StartVotingEvent)} event.");
    }
}