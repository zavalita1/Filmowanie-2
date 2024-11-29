using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Events;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Database.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Sagas;

public sealed class VotingStateMachine : MassTransitStateMachine<VotingStateInstance>
{
    private readonly ILogger<VotingStateMachine> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;

    public VotingStateMachine(ILogger<VotingStateMachine> logger, IDateTimeProvider dateTimeProvider)
    {
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;

        InstanceState(x => x.CurrentState);

        Event(() => StartVotingEvent, x => x.CorrelateById(y => y.Message.CorrelationId));
        Event(() => InitNominationsEvent, x => x.CorrelateById(y => y.Message.CorrelationId));
        Event(() => AddMovieEvent, x => x.CorrelateById(y => y.Message.CorrelationId));
        Event(() => RemoveMovieEvent, x => x.CorrelateById(y => y.Message.CorrelationId));
        Event(() => ConcludeVoting, x => x.CorrelateById(y => y.Message.CorrelationId));
        Event(() => GetMovieListEvent, x => x.CorrelateById(y => y.Message.CorrelationId));

        Initially(
            When(StartVotingEvent)
                .Activity(x => x.OfType<CreateVotingSessionEntryActivity>())
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
                    var nominationToConclude = ctx.Saga.Nominations.Single(x => x.User.Id == ctx.Message.User.Id);
                    nominationToConclude.Concluded = _dateTimeProvider.Now;
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
                    var nominationData = ctx.Saga.Nominations.Single(x => x.Year == year);
                    nominationData.Concluded = null;
                    await ctx.Publish(new NominationAddedEvent(ctx.Message.CorrelationId, nominationData), ctx.CancellationToken);

                    var currentState = await Accessor.Get(ctx);
                    if (currentState.Name == nameof(NominationsConcluded))
                        await ctx.TransitionToState(WaitingForNominations);
                })
        );

        During([WaitingForNominations, NominationsConcluded],
            When(AddVoteEvent)
                .Then(ctx =>
                {
                    var movie = ctx.Saga.Movies.Single(x => x.id == ctx.Message.Movie.id);
                    var messageUser = ctx.Message.User;

                    if (movie.Votes.Any(x => x.User.id == messageUser.Id))
                        return;

                    var user = new ReadOnlyEmbeddedUser(messageUser.Id, messageUser.DisplayName, messageUser.Tenant.Id);
                    movie.Votes = movie.Votes.Concat([new Vote(user, ctx.Message.VoteType)]);
                })
                .Publish(ctx => new VoteAddedEvent(ctx.Message.CorrelationId, ctx.Message.Movie, ctx.Message.User)));

        During([WaitingForNominations, NominationsConcluded],
            When(RemoveVoteEvent)
                .Then(ctx =>
                {
                    var movie = ctx.Saga.Movies.Single(x => x.id == ctx.Message.Movie.id);
                    var messageUser = ctx.Message.User;

                    if (movie.Votes.All(x => x.User.id != messageUser.Id))
                        return;

                    movie.Votes = movie.Votes.Where(x => x.User.id != messageUser.Id).ToArray();
                })
                .Publish(ctx => new VoteAddedEvent(ctx.Message.CorrelationId, ctx.Message.Movie, ctx.Message.User)));

        During(NominationsConcluded,
            When(ConcludeVoting)
                .Then(ctx => _logger.LogWarning("Voting ending..."))
                .Publish(ctx => new VotingConcludedEvent(ctx.Message.CorrelationId))
                .Finalize()
        );

        During([WaitingForNominations, NominationsConcluded],
            When(GetMovieListEvent)
                .Respond(ctx => new CurrentVotingListResponse { Movies = ctx.Saga.Movies.Cast<IReadOnlyEmbeddedMovieWithVotes>().ToArray() }));

        During([WaitingForNominations, NominationsConcluded],
            When(GeNominationsEvent)
                .Respond(ctx => new CurrentNominationsResponse { Nominations = ctx.Saga.Nominations.ToArray() }));
    }

    // Commands
    public Event<StartVotingEvent> StartVotingEvent { get; private set; } = null!;

    public Event<AddNominationsEvent> InitNominationsEvent { get; private set; } = null!;

    public Event<AddMovieEvent> AddMovieEvent { get; private set; } = null!;

    public Event<RemoveMovieEvent> RemoveMovieEvent { get; private set; } = null!;

    public Event<AddVoteEvent> AddVoteEvent { get; private set; } = null!;

    public Event<RemoveVoteEvent> RemoveVoteEvent { get; private set; } = null!;

    public Event<ConcludeVotingEvent> ConcludeVoting { get; private set; } = null!;

    // Events

    public Event<MoviesListRequested> GetMovieListEvent { get; private set; } = null!;

    public Event<NominationsRequested> GeNominationsEvent { get; private set; } = null!;

    // States

    public State WaitingForNominations { get; private set; } = null!;

    public State NominationsConcluded { get; private set; } = null!;
}

public class CurrentVotingListResponse
{
    public IReadOnlyEmbeddedMovieWithVotes[] Movies { get; set; }
}

public class CurrentNominationsResponse
{
    public NominationData[] Nominations { get; set; }
}

public readonly record struct ReadOnlyEmbeddedUser(string id, string Name, int TenantId) : IReadOnlyEmbeddedUser;