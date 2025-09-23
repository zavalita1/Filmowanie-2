using Filmowanie.Abstractions.Extensions;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Notification.Services;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Notification.Consumers;

// TODO UTs
public sealed class MovieAddedConsumer : IConsumer<AddMovieEvent>, IConsumer<Fault<AddMovieEvent>>
{
    private readonly ILogger<MovieAddedConsumer> logger;
    private readonly IHubContext<VotingStateHub> votingHubContext;

    public MovieAddedConsumer(ILogger<MovieAddedConsumer> logger, IHubContext<VotingStateHub> votingHubContext)
    {
        this.logger = logger;
        this.votingHubContext = votingHubContext;
    }

    public Task Consume(ConsumeContext<Fault<AddMovieEvent>> context)
    {
        var message = context.Message.Exceptions.Select(x => x.Message).JoinStrings();
        var callStacks = context.Message.Exceptions.Select(x => x.StackTrace).JoinStrings(";;;;;;;;;;;;");
        return context.Publish(new ErrorEvent(context.Message.Message.VotingSessionId, message, callStacks));
    }

    public async Task Consume(ConsumeContext<AddMovieEvent> context)
    {
        this.logger.LogInformation($"Consuming {nameof(AddMovieEvent)}...");

        var user = context.Message.User;
        await this.votingHubContext.Clients.All.SendAsync("movie nominated", new { user.Name, Gender = user.Gender.ToString() } , context.CancellationToken);
        this.logger.LogInformation($"Consumed {nameof(VotingConcludedEvent)} event.");
    }
}