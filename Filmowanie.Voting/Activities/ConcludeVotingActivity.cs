using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.Interfaces;
using Filmowanie.Voting.Sagas;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Activities;

// TODO UTs
public class ConcludeVotingActivity : IStateMachineActivity<VotingStateInstance, ConcludeVotingEvent>
{
    private readonly ILogger<ConcludeVotingActivity> logger;
    private readonly IBus bus;
    private readonly IVotingResultInterpreter votingResultInterpreter;


    public ConcludeVotingActivity(ILogger<ConcludeVotingActivity> logger, IBus bus, IVotingResultInterpreter votingResultInterpreter)
    {
        this.logger = logger;
        this.bus = bus;
        this.votingResultInterpreter = votingResultInterpreter;
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("voting-add-movie");
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<VotingStateInstance, ConcludeVotingEvent> ctx, IBehavior<VotingStateInstance, ConcludeVotingEvent> next)
    {
        this.logger.LogInformation("Executing concluding voting activity..");

        var isExtraVoting = ctx.Saga.CurrentState == nameof(VotingStateMachine.ExtraVoting);
        if (isExtraVoting || !this.votingResultInterpreter.IsExtraVotingNecessary(ctx.Saga.Movies.ToArray(), out var extraVotingMovies))
        {
            var movies = ctx.Saga.Movies.Cast<IReadOnlyEmbeddedMovieWithVotes>().ToArray();
            var nominations = ctx.Saga.Nominations.ToArray();
            var @event = new VotingConcludedEvent(ctx.Message.VotingSessionId, ctx.Message.Tenant, movies, nominations, ctx.Saga.Created, ctx.Saga.ExtraVotingMovies?.ToArray());
            await this.bus.Publish(@event);
            var nextState = isExtraVoting ? ctx.StateMachine.GetState(nameof(VotingStateMachine.CalculatingExtraResults)) : ctx.StateMachine.GetState(nameof(VotingStateMachine.CalculatingResults));
            await ctx.TransitionToState(nextState);
        }
        else
        {
            ctx.Saga.ExtraVotingMovies = extraVotingMovies.Select(x => new EmbeddedMovieWithVotes(x));
            await ctx.TransitionToState(ctx.StateMachine.GetState(nameof(VotingStateMachine.ExtraVoting)));
        }
;
        this.logger.LogInformation("Executed concluding voting activity.");
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<VotingStateInstance, ConcludeVotingEvent, TException> context, IBehavior<VotingStateInstance, ConcludeVotingEvent> next) where TException : Exception
    {
        await next.Faulted(context);
    }
}