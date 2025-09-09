using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Database.Interfaces;
using Filmowanie.Voting.Sagas;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Activities;

public class CreateVotingSessionEntryActivity : IStateMachineActivity<VotingStateInstance, StartVotingEvent>
{
    private readonly ILogger<VotingStateMachine> _logger;
    private readonly IVotingResultsCommandRepository _votingSessionCommandRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateVotingSessionEntryActivity(ILogger<VotingStateMachine> logger, IVotingResultsCommandRepository votingSessionCommandRepository, IDateTimeProvider dateTimeProvider)
    {
        _logger = logger;
        _votingSessionCommandRepository = votingSessionCommandRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("voting-start-entry-in-db");
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<VotingStateInstance, StartVotingEvent> context, IBehavior<VotingStateInstance, StartVotingEvent> next)
    {
        _logger.LogInformation("Adding Voting Session entry in db..");
        var votingResult = new VotingResult { Created = _dateTimeProvider.Now, id = context.Saga.CorrelationId.ToString(), TenantId = context.Message.TenantId.Id, Movies = [], UsersAwardedWithNominations = [] };
        await _votingSessionCommandRepository.InsertAsync(votingResult, context.CancellationToken);
        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<VotingStateInstance, StartVotingEvent, TException> context, IBehavior<VotingStateInstance, StartVotingEvent> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }
}