using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Database.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Consumers;

public sealed class ResumeVotingEventConsumer : IConsumer<ResumeVotingEvent>, IConsumer<Fault<ResumeVotingEvent>>
{
    private readonly ILogger<ResumeVotingEventConsumer> log;

    private readonly IVotingResultsCommandRepository votingResultsCommandRepository;

    public ResumeVotingEventConsumer(ILogger<ResumeVotingEventConsumer> log, IVotingResultsCommandRepository votingResultsCommandRepository)
    {
        this.log = log;
        this.votingResultsCommandRepository = votingResultsCommandRepository;
    }

    public Task Consume(ConsumeContext<Fault<ResumeVotingEvent>> context)
    {
        this.log.LogError($"Processing fault in {nameof(ResumeVotingEventConsumer)}...");
        var message = context.Message.Exceptions.Select(x => x.Message).JoinStrings();
        var callStacks = context.Message.Exceptions.Select(x => x.StackTrace).JoinStrings(";;;;;;;;;;;;");
        return context.Publish(new ErrorEvent(context.Message.Message.VotingSessionId, message, callStacks));
    }

    public async Task Consume(ConsumeContext<ResumeVotingEvent> context)
    {
        this.log.LogInformation($"Consuming {nameof(ResumeVotingEvent)}...");
        try
        {
            var result = await this.votingResultsCommandRepository.ResetAsync(context.Message.VotingSessionId, context.CancellationToken);

            if (result.Error.HasValue)
                await PublishErrorAsync(context, error: result.Error);
        }
        catch (Exception ex)
        {
            await PublishErrorAsync(context, ex);
        }

        this.log.LogInformation($"Consumed {nameof(ResumeVotingEvent)}.");
    }
    
      private Task PublishErrorAsync(ConsumeContext<ResumeVotingEvent> context, Exception? ex = null, Error<VoidResult>? error = null)
    {
        var msg = "Error occurred during resuming the voting..." + error?.ToString();

        if (ex == null)
            this.log.LogError(msg);
        else
            this.log.LogError(ex, msg);

        var errorEvent = new ErrorEvent(context.Message.VotingSessionId, msg, ex?.StackTrace ?? "Unknown");
        return context.Publish(errorEvent, context.CancellationToken);
    }
}
