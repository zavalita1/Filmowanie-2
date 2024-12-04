using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.Activities;
using Filmowanie.Voting.Extensions;
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
                    var movies = ctx.Message.Movies.Select(x => new EmbeddedMovieWithVotes { Movie = x, Votes = Array.Empty<Vote>() }).ToArray();
                    ctx.Saga.Movies = movies;
                    ctx.Saga.Nominations = ctx.Message.NominationsData;
                    ctx.Saga.TenantId = ctx.Message.TenantId.Id;
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
                    var movieWithVotes = new EmbeddedMovieWithVotes(ctx.Message.Movie);
                    ctx.Saga.Movies = ctx.Saga.Movies.Concat([movieWithVotes]);

                    var nominationToConclude = ctx.Saga.Nominations.Single(x => x.Year == ctx.Message.Decade);
                    nominationToConclude.Concluded = _dateTimeProvider.Now;
                    nominationToConclude.MovieId = ctx.Message.Movie.id;
                    return ctx.Saga.Nominations.All(x => x.Concluded == null) ? Task.CompletedTask : ctx.TransitionToState(NominationsConcluded);
                }));

        During([WaitingForNominations, NominationsConcluded],
            When(RemoveMovieEvent)
                .Then(ctx => _logger.LogInformation("Removing movie..."))
                .ThenAsync(async ctx =>
                {
                    if (ctx.Saga.Movies.All(x => x.Movie.id != ctx.Message.Movie.id))
                        return;

                    var nominationData = ctx.Saga.Nominations.Single(x => x.Year == ctx.Message.Movie.MovieCreationYear.ToDecade());
                    nominationData.Concluded = null;
                    nominationData.MovieId = null;
                    await ctx.Publish(new NominationAddedEvent(ctx.Message.CorrelationId, nominationData), ctx.CancellationToken);
                    ctx.Saga.Movies = ctx.Saga.Movies.Where(x => x.Movie.id != ctx.Message.Movie.id).ToArray();

                    var currentState = await Accessor.Get(ctx);
                    if (currentState.Name == nameof(NominationsConcluded))
                        await ctx.TransitionToState(WaitingForNominations);
                })
        );

        During([WaitingForNominations, NominationsConcluded],
            When(AddVoteEvent)
                .Then(ctx =>
                {
                    var movie = ctx.Saga.Movies.Single(x => x.Movie.id == ctx.Message.Movie.id);
                    var messageUser = ctx.Message.User;

                    if (movie.Votes.Any(x => x.User.id == messageUser.Id))
                        return;

                    var user = new EmbeddedUser { id = messageUser.Id, Name = messageUser.DisplayName, TenantId = messageUser.Tenant.Id };
                    movie.Votes = movie.Votes.Concat([new Vote { User = user, VoteType = ctx.Message.VoteType }]);
                    movie.VotingScore += ctx.Message.VoteType.GetVoteCount();
                })
                .Publish(ctx => new VoteAddedEvent(ctx.Message.CorrelationId, ctx.Message.Movie, ctx.Message.User)));

        During([WaitingForNominations, NominationsConcluded],
            When(RemoveVoteEvent)
                .Then(ctx =>
                {
                    var movie = ctx.Saga.Movies.Single(x => x.Movie.id == ctx.Message.Movie.id);
                    var messageUser = ctx.Message.User;

                    if (movie.Votes.All(x => x.User.id != messageUser.Id))
                        return;

                    var voteToRemove = movie.Votes.Single(x => x.User.id == messageUser.Id);
                    movie.Votes = movie.Votes.Except([voteToRemove]).ToArray();
                    movie.VotingScore -= voteToRemove.VoteType.GetVoteCount();
                })
                .Publish(ctx => new VoteAddedEvent(ctx.Message.CorrelationId, ctx.Message.Movie, ctx.Message.User)));

        During(NominationsConcluded,
            When(ConcludeVoting)
                .Then(ctx => _logger.LogWarning("Voting ending..."))
                .Publish(ctx =>
                {
                    var movies = ctx.Saga.Movies.Cast<IReadOnlyEmbeddedMovieWithVotes>().ToArray();
                    var nominations = ctx.Saga.Nominations.ToArray();
                    return new VotingConcludedEvent(ctx.Message.CorrelationId, ctx.Message.Tenant, movies, nominations, ctx.Saga.Created);
                })
                .TransitionTo(CalculatingResults)
        );

        During(CalculatingResults, When(ResultsCalculatedEvent).Finalize());
        During(CalculatingResults, When(Error)
            .Then(ctx => ctx.Saga.Error = new ErrorData { ErrorMessage = ctx.Message.Message, CallStack = ctx.Message.CallStack })
            .TransitionTo(NominationsConcluded));

        During([WaitingForNominations, NominationsConcluded],
            When(GetMovieListEvent)
                .Respond(ctx => new CurrentVotingListResponse { Movies = ctx.Saga.Movies.Cast<IReadOnlyEmbeddedMovieWithVotes>().ToArray() }));

        During([WaitingForNominations, NominationsConcluded],
            When(GetNominationsEvent)
                .Respond(ctx => new CurrentNominationsResponse { Nominations = ctx.Saga.Nominations.ToArray(), CorrelationId = ctx.Saga.CorrelationId }));
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
    public Event<NominationsRequested> GetNominationsEvent { get; private set; } = null!;
    public Event<ResultsCalculated> ResultsCalculatedEvent { get; private set; } = null!;
    public Event<ErrorEvent> Error { get; private set; } = null!;


    // States
    public State WaitingForNominations { get; private set; } = null!;

    public State NominationsConcluded { get; private set; } = null!;

    public State CalculatingResults { get; private set; } = null!;
}
