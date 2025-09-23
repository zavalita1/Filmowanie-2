using Filmowanie.Abstractions.Extensions;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Notification.Services;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Notification.Consumers;

// TODO UTs
public sealed class StartVotingEventConsumer : IConsumer<StartVotingEvent>, IConsumer<Fault<StartVotingEvent>>
{
    private readonly ILogger<StartVotingEventConsumer> logger;
    private readonly IHubContext<VotingStateHub> votingHubContext;

    public StartVotingEventConsumer(ILogger<StartVotingEventConsumer> logger, IHubContext<VotingStateHub> votingHubContext)
    {
        this.logger = logger;
        this.votingHubContext = votingHubContext;
    }

    public Task Consume(ConsumeContext<Fault<StartVotingEvent>> context)
    {
        var message = context.Message.Exceptions.Select(x => x.Message).JoinStrings();
        var callStacks = context.Message.Exceptions.Select(x => x.StackTrace).JoinStrings(";;;;;;;;;;;;");
        return context.Publish(new ErrorEvent(context.Message.Message.VotingSessionId, message, callStacks));
    }

    public async Task Consume(ConsumeContext<StartVotingEvent> context)
    {
        this.logger.LogInformation($"Consuming {nameof(StartVotingEvent)}...");
        await this.votingHubContext.Clients.All.SendAsync("voting started", context.CancellationToken);
        this.logger.LogInformation($"Consumed {nameof(StartVotingEvent)} event.");
    }
}