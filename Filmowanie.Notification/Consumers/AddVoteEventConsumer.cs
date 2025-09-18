using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Notification.Services;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Notification.Consumers;

// TODO UTs
public sealed class AddVoteEventConsumer : IConsumer<AddVoteEvent>, IConsumer<Fault<AddVoteEvent>>
{
    private readonly ILogger<AddVoteEventConsumer> _logger;
    private readonly IHubContext<VotingStateHub> _votingHubContext;

    public AddVoteEventConsumer(ILogger<AddVoteEventConsumer> logger, IHubContext<VotingStateHub> votingHubContext)
    {
        _logger = logger;
        _votingHubContext = votingHubContext;
    }

    public Task Consume(ConsumeContext<Fault<AddVoteEvent>> context)
    {
        var message = string.Join(",", context.Message.Exceptions.Select(x => x.Message));
        var callStacks = string.Join(";;;;;;;;;;;;", context.Message.Exceptions.Select(x => x.StackTrace));
        return context.Publish(new ErrorEvent(context.Message.Message.VotingSessionId, message, callStacks));
    }

    public async Task Consume(ConsumeContext<AddVoteEvent> context)
    {
        _logger.LogInformation($"Consuming {nameof(AddVoteEvent)}...");
        var userId = context.Message.User;
        await _votingHubContext.Clients.All.SendAsync("voted", new { userId.Name, Gender = context.Message.User.Gender.ToString() }, context.CancellationToken);
        _logger.LogInformation($"Consumed {nameof(AddVoteEvent)} event.");
    }
}