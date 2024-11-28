using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Events;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Sagas;

public class VotingStateMachine : MassTransitStateMachine<VotingStateInstance>
{
    private readonly ILogger<VotingStateMachine> _logger;

    // Commands
    public Event<StartVotingEvent> StartVotingEvent { get; private set; } = null!;
    public Event<AddNominationsEvent> InitNominationsEvent { get; private set; } = null!;
    public Event<AddMovieEvent> AddMovieEvent { get; private set; } = null!;
    public Event<RemoveMovieEvent> RemoveMovieEvent { get; private set; } = null!;
    public Event<VotingConcludedEvent> ConcludeVoting { get; private set; } = null!;

    // Events
    public Event<MoviesListRequested> GetMovieListEvent { get; private set; } = null!;
    public Event<NominationsRequested> GeNominationsEvent { get; private set; } = null!;

    // States
    public State WaitingForNominations { get; private set; } = null!;
    public State NominationsConcluded { get; private set; } = null!;

    public VotingStateMachine(ILogger<VotingStateMachine> logger)
    {
        _logger = logger;

        InstanceState(x => x.CurrentState);

        Event(() => StartVotingEvent, x => x.CorrelateById(y => y.Message.CorrelationId));
        Event(() => InitNominationsEvent, x => x.CorrelateById(y => y.Message.CorrelationId));
        Event(() => AddMovieEvent, x => x.CorrelateById(y => y.Message.CorrelationId));
        Event(() => RemoveMovieEvent, x => x.CorrelateById(y => y.Message.CorrelationId));
        Event(() => ConcludeVoting, x => x.CorrelateById(y => y.Message.CorrelationId));
        Event(() => GetMovieListEvent, x => x.CorrelateById(y => y.Message.CorrelationId));

        Initially(
            When(StartVotingEvent)
                .Then(ctx =>
                {
                    _logger.LogInformation("Voting is starting...");
                    var movies = ctx.Message.Movies.Select(x => new EmbeddedMovieWithVotes { id = x.id, Name = x.Name, Votes = Array.Empty<Vote>()}).ToArray();
                    ctx.Saga.Movies = movies;
                    ctx.Saga.Nominations = ctx.Message.NominationsData;
                })
                 .ThenAsync(async ctx =>
                {
                    foreach (var nominationData in ctx.Message.NominationsData)
                    {
                        await ctx.Publish(new NominationAddedEvent(ctx.Message.CorrelationId, nominationData), ctx.CancellationToken);
                    }
                })
                .Publish(ctx => new VotingStartingEvent(ctx.Saga.CorrelationId))
                .TransitionTo(WaitingForNominations));

        During(WaitingForNominations,
            When(AddMovieEvent)
                .Then(ctx => _logger.LogInformation("Adding movie.."))
                .ThenAsync(ctx =>
                {
                    var nominationToRemove = ctx.Saga.Nominations.SingleOrDefault(x => x.User.Id == ctx.Message.User.Id);

                    if (nominationToRemove == null)
                        return Task.CompletedTask;

                    ctx.Saga.Nominations = ctx.Saga.Nominations.Except([nominationToRemove]).ToArray();
                    return ctx.Saga.Nominations.Any() ? Task.CompletedTask : ctx.TransitionToState(NominationsConcluded);
                }));

        During([WaitingForNominations, NominationsConcluded],
            When(RemoveMovieEvent)
                .Then(ctx => _logger.LogInformation("Removing movie..."))
                .ThenAsync(async ctx =>
                {
                    if (ctx.Saga.Movies.All(x => x.id != ctx.Message.Movie.id))
                        return;

                    var year = 1900;// TODO get proper;
                    var nominationData = new TempNominationData(ctx.Message.User, year);
                    ctx.Saga.Nominations = ctx.Saga.Nominations.Concat([nominationData]);
                    await ctx.Publish(new NominationAddedEvent(ctx.Message.CorrelationId, nominationData), ctx.CancellationToken);

                    var currentState = await Accessor.Get(ctx);
                    if (currentState.Name == nameof(NominationsConcluded))
                        await ctx.TransitionToState(WaitingForNominations);
                })
        );

        During(NominationsConcluded,
            When(ConcludeVoting)
                .Then(ctx => _logger.LogWarning("Voting ending..."))
                .Finalize()
        );

        During([WaitingForNominations, NominationsConcluded],
            When(GetMovieListEvent)
                .Respond(ctx => new CurrentVotingListResponse { Movies = ctx.Saga.Movies.Cast<IReadOnlyEmbeddedMovieWithVotes>().ToArray() }));

        During([WaitingForNominations, NominationsConcluded],
            When(GeNominationsEvent)
                .Respond(ctx => new CurrentNominationsResponse { Nominations = ctx.Saga.Nominations.ToArray() }));
    }
}

public class CurrentVotingListResponse
{
    public IReadOnlyEmbeddedMovieWithVotes[] Movies { get; set; }
}

public class CurrentNominationsResponse
{
    public TempNominationData[] Nominations { get; set; }
}