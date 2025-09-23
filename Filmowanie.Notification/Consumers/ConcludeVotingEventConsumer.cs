using Filmowanie.Abstractions.Extensions;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Notification.Services;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Notification.Consumers;

// TODO UTs
public sealed class ConcludeVotingEventConsumer : IConsumer<ConcludeVotingEvent>, IConsumer<Fault<ConcludeVotingEvent>>
{
    private readonly ILogger<ConcludeVotingEventConsumer> logger;
    private readonly IHubContext<VotingStateHub> votingHubContext;

    public ConcludeVotingEventConsumer(ILogger<ConcludeVotingEventConsumer> logger, IHubContext<VotingStateHub> votingHubContext)
    {
        this.logger = logger;
        this.votingHubContext = votingHubContext;
    }

    public Task Consume(ConsumeContext<Fault<ConcludeVotingEvent>> context)
    {
        var message = context.Message.Exceptions.Select(x => x.Message).JoinStrings();
        var callStacks = context.Message.Exceptions.Select(x => x.StackTrace).JoinStrings(";;;;;;;;;;;;");
        return context.Publish(new ErrorEvent(context.Message.Message.VotingSessionId, message, callStacks));
    }

    public async Task Consume(ConsumeContext<ConcludeVotingEvent> context)
    {
        this.logger.LogInformation($"Consuming {nameof(ConcludeVotingEvent)}...");
        await this.votingHubContext.Clients.All.SendAsync("voting ended", context.CancellationToken);
        this.logger.LogInformation($"Consumed {nameof(ConcludeVotingEvent)} event.");
    }
}