using Filmowanie.Database.Entities.Voting;
using Filmowanie.Notification.Services;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Notification.Consumers;

public sealed class MovieAddedConsumer : IConsumer<AddMovieEvent>, IConsumer<Fault<AddMovieEvent>>
{
    private readonly ILogger<MovieAddedConsumer> _logger;
    private readonly IHubContext<VotingStateHub> _votingHubContext;

    public MovieAddedConsumer(ILogger<MovieAddedConsumer> logger, IHubContext<VotingStateHub> votingHubContext)
    {
        _logger = logger;
        _votingHubContext = votingHubContext;
    }

    public Task Consume(ConsumeContext<Fault<AddMovieEvent>> context)
    {
        var message = string.Join(",", context.Message.Exceptions.Select(x => x.Message));
        var callStacks = string.Join(";;;;;;;;;;;;", context.Message.Exceptions.Select(x => x.StackTrace));
        return context.Publish(new ErrorEvent(context.Message.Message.CorrelationId, message, callStacks));
    }

    public async Task Consume(ConsumeContext<AddMovieEvent> context)
    {
        _logger.LogInformation($"Consuming {nameof(AddMovieEvent)}...");

        var user = context.Message.User;
        await _votingHubContext.Clients.All.SendAsync("movie nominated", user.Name, context.CancellationToken);
        _logger.LogInformation($"Consumed {nameof(VotingConcludedEvent)} event.");
    }
}