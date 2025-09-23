using Filmowanie.Abstractions.Extensions;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Notification.Services;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Notification.Consumers;

// TODO UTs
public sealed class RemoveVoteEventConsumer : IConsumer<RemoveVoteEvent>, IConsumer<Fault<RemoveVoteEvent>>
{
    private readonly ILogger<RemoveVoteEventConsumer> logger;
    private readonly IHubContext<VotingStateHub> votingHubContext;

    public RemoveVoteEventConsumer(ILogger<RemoveVoteEventConsumer> logger, IHubContext<VotingStateHub> votingHubContext)
    {
        this.logger = logger;
        this.votingHubContext = votingHubContext;
    }

    public Task Consume(ConsumeContext<Fault<RemoveVoteEvent>> context)
    {
        var message = context.Message.Exceptions.Select(x => x.Message).JoinStrings();
        var callStacks = context.Message.Exceptions.Select(x => x.StackTrace).JoinStrings(";;;;;;;;;;;;");
        return context.Publish(new ErrorEvent(context.Message.Message.VotingSessionId, message, callStacks));
    }

    public async Task Consume(ConsumeContext<RemoveVoteEvent> context)
    {
        this.logger.LogInformation($"Consuming {nameof(RemoveVoteEvent)}...");
        var userId = context.Message.User;
        await this.votingHubContext.Clients.All.SendAsync("voted", new { userId.Name, Gender = context.Message.User.Gender.ToString() }, context.CancellationToken);
        this.logger.LogInformation($"Consumed {nameof(RemoveVoteEvent)} event.");
    }
}