using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.Sagas;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Activities;

// TODO UTs
public class RemoveMovieActivity : IStateMachineActivity<VotingStateInstance, RemoveMovieEvent>
{
    private readonly ILogger<RemoveMovieActivity> logger;
    private readonly IBus bus;
    private readonly IDateTimeProvider dateTimeProvider;
    private readonly IRepositoryInUserlessContextProvider repositoryInUserlessContextProvider;

    public RemoveMovieActivity(ILogger<RemoveMovieActivity> logger, IBus bus, IDateTimeProvider dateTimeProvider, IRepositoryInUserlessContextProvider repositoryInUserlessContextProvider)
    {
        this.logger = logger;
        this.bus = bus;
        this.dateTimeProvider = dateTimeProvider;
        this.repositoryInUserlessContextProvider = repositoryInUserlessContextProvider;
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("voting-remove-movie");
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<VotingStateInstance, RemoveMovieEvent> ctx, IBehavior<VotingStateInstance, RemoveMovieEvent> next)
    {
        this.logger.LogInformation("Removing movie..");
        if (ctx.Saga.Movies.All(x => x.Movie.id != ctx.Message.Movie.id))
            return;

        var nominationData = ctx.Saga.Nominations.SingleOrDefault(x => x.Year == ctx.Message.Movie.MovieCreationYear.ToDecade());

        if (nominationData == null)
        {
            var batchSize = 10;
            var counter = 1;
            IReadOnlyEmbeddedUser? nominatedBy = null;

            while (nominatedBy == null)
            {
                var repo = this.repositoryInUserlessContextProvider.GetVotingResultsRepository(ctx.Message.User.Tenant);
                var lastVotingResults = await repo.GetLastNVotingResultsAsync(counter * batchSize, ctx.CancellationToken);

                if (lastVotingResults.Error != null)
                    throw new Exception(lastVotingResults.Error.ToString());

                nominatedBy = lastVotingResults.Result!
                    .Select(x => x.MoviesAdded.SingleOrDefault(y => y.Movie.id == ctx.Message.Movie.id))
                    .SingleOrDefault(x => x != null)
                    ?.NominatedBy;
                    
                counter++;
            }

            var userData = new NominationDataEmbeddedUser { Id = nominatedBy.id, DisplayName = nominatedBy.Name };
            nominationData = new NominationData { Concluded = null, MovieId = null, User = userData, Year = ctx.Message.Movie.MovieCreationYear.ToDecade() };
        }
        else
        {
            nominationData.Concluded = null;
            nominationData.MovieId = null;
        }

        ctx.Saga.Nominations = ctx.Saga.Nominations.Concat([nominationData]);
        await ctx.Publish(new NominationAddedEvent(ctx.Message.VotingSessionId, nominationData), ctx.CancellationToken);
        ctx.Saga.Movies = ctx.Saga.Movies.Where(x => x.Movie.id != ctx.Message.Movie.id).ToArray();

        var currentState = ctx.Saga.CurrentState;
        if (currentState == nameof(VotingStateMachine.NominationsConcluded))
            await ctx.TransitionToState(ctx.StateMachine.GetState(nameof(VotingStateMachine.WaitingForNominations)));
    }

    public async Task Faulted<TException>(BehaviorExceptionContext<VotingStateInstance, RemoveMovieEvent, TException> context, IBehavior<VotingStateInstance, RemoveMovieEvent> next) where TException : Exception
    {
        await next.Faulted(context).ConfigureAwait(false);
    }
}