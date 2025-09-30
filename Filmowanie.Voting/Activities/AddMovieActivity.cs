using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Voting.Sagas;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Activities;

// TODO UTs
public class AddMovieActivity : IStateMachineActivity<VotingStateInstance, AddMovieEvent>
{
    private readonly ILogger<AddMovieActivity> logger;
    private readonly IBus bus;
    private readonly IDateTimeProvider dateTimeProvider;

    public AddMovieActivity(ILogger<AddMovieActivity> logger, IBus bus, IDateTimeProvider dateTimeProvider)
    {
        this.logger = logger;
        this.bus = bus;
        this.dateTimeProvider = dateTimeProvider;
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("voting-add-movie");
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<VotingStateInstance, AddMovieEvent> ctx, IBehavior<VotingStateInstance, AddMovieEvent> next)
    {
        this.logger.LogInformation("Adding movie..");
        var movieWithVotes = new EmbeddedMovieWithVotes(ctx.Message.Movie);
        ctx.Saga.Movies = ctx.Saga.Movies.Concat([movieWithVotes]);

        var nominationToConclude = ctx.Saga.Nominations.Single(x => x.Year == ctx.Message.Decade);
        nominationToConclude.Concluded = this.dateTimeProvider.Now;
        nominationToConclude.MovieId = ctx.Message.Movie.id;

        var result = ctx.Saga.Nominations.All(x => x.Concluded != null) ? ctx.TransitionToState(ctx.StateMachine.GetState(nameof(VotingStateMachine.NominationsConcluded))) : Task.CompletedTask;
        await result.ConfigureAwait(false);

        var @event = new MovieAddedEvent(ctx.Message.VotingSessionId, ctx.Message.Movie);
        await this.bus.Publish(@event);
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<VotingStateInstance, AddMovieEvent, TException> context, IBehavior<VotingStateInstance, AddMovieEvent> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }
}