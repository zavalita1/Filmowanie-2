using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Database.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Activities;

// TODO UTs
public class CreateVotingSessionEntryActivity : IStateMachineActivity<VotingStateInstance, StartVotingEvent>
{
    private readonly ILogger<CreateVotingSessionEntryActivity> logger;
    private readonly IVotingResultsCommandRepository votingSessionCommandRepository;
    private readonly IDateTimeProvider dateTimeProvider;

    public CreateVotingSessionEntryActivity(ILogger<CreateVotingSessionEntryActivity> logger, IVotingResultsCommandRepository votingSessionCommandRepository, IDateTimeProvider dateTimeProvider)
    {
        this.logger = logger;
        this.votingSessionCommandRepository = votingSessionCommandRepository;
        this.dateTimeProvider = dateTimeProvider;
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
        this.logger.LogInformation("Adding Voting Session entry in db..");
        var votingResult = new VotingResult { Created = this.dateTimeProvider.Now, id = context.Saga.CorrelationId.ToString(), TenantId = context.Message.TenantId.Id, Movies = [], UsersAwardedWithNominations = [] };
        await this.votingSessionCommandRepository.InsertAsync(votingResult, context.CancellationToken);
        await next.Execute(context).ConfigureAwait(false);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<VotingStateInstance, StartVotingEvent, TException> context, IBehavior<VotingStateInstance, StartVotingEvent> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }
}