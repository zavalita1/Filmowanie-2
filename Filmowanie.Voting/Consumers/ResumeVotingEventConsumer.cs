using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Database.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Consumers;

public sealed class ResumeVotingEventConsumer : IConsumer<ResumeVotingEvent>, IConsumer<Fault<ResumeVotingEvent>>
{
    private readonly ILogger<ResumeVotingEventConsumer> _log;

    private readonly IVotingResultsCommandRepository _votingResultsCommandRepository;

    public ResumeVotingEventConsumer(ILogger<ResumeVotingEventConsumer> log, IVotingResultsCommandRepository votingResultsCommandRepository)
    {
        _log = log;
        _votingResultsCommandRepository = votingResultsCommandRepository;
    }

    public Task Consume(ConsumeContext<Fault<ResumeVotingEvent>> context)
    {
        _log.LogError($"Processing fault in {nameof(ResumeVotingEventConsumer)}...");
        var message = string.Join(",", context.Message.Exceptions.Select(x => x.Message));
        var callStacks = string.Join(";;;;;;;;;;;;", context.Message.Exceptions.Select(x => x.StackTrace));
        return context.Publish(new ErrorEvent(context.Message.Message.VotingSessionId, message, callStacks));
    }

    public async Task Consume(ConsumeContext<ResumeVotingEvent> context)
    {
        _log.LogInformation($"Consuming {nameof(VotingConcludedEvent)}...");
        try
        {
            var result = await _votingResultsCommandRepository.ResetAsync(context.Message.VotingSessionId, context.CancellationToken);

            if (result.Error.HasValue)
                await PublishErrorAsync(context, error: result.Error);
        }
        catch (Exception ex)
        {
            await PublishErrorAsync(context, ex);
        }
        
         _log.LogInformation($"Consumed {nameof(VotingConcludedEvent)}.");
    }
    
      private Task PublishErrorAsync(ConsumeContext<ResumeVotingEvent> context, Exception? ex = null, Error<VoidResult>? error = null)
    {
        var msg = "Error occurred during resuming the voting..." + error?.ToString();

        if (ex == null)
            _log.LogError(msg);
        else
            _log.LogError(ex, msg);

        var errorEvent = new ErrorEvent(context.Message.VotingSessionId, msg, ex?.StackTrace ?? "Unknown");
        return context.Publish(errorEvent, context.CancellationToken);
    }
}
