using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.Activities;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.Extensions;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Sagas;

// TODO UTs
public sealed class VotingStateMachine : MassTransitStateMachine<VotingStateInstance>
{
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly ILogger<VotingStateMachine> logger;
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly IDateTimeProvider dateTimeProvider;

    public VotingStateMachine(ILogger<VotingStateMachine> logger, IDateTimeProvider dateTimeProvider)
    {
        this.logger = logger;
        this.dateTimeProvider = dateTimeProvider;

        InstanceState(x => x.CurrentState);

        Event(() => StartVotingEvent, x => x.CorrelateById(y => y.Message.VotingSessionId.CorrelationId));
        Event(() => InitNominationsEvent, x => x.CorrelateById(y => y.Message.VotingSessionId.CorrelationId));
        Event(() => AddMovieEvent, x => x.CorrelateById(y => y.Message.VotingSessionId.CorrelationId));
        Event(() => MovieAddedEvent, x => x.CorrelateById(y => y.Message.VotingSessionId.CorrelationId));
        Event(() => RemoveMovieEvent, x => x.CorrelateById(y => y.Message.VotingSessionId.CorrelationId));
        Event(() => AddVoteEvent, x => x.CorrelateById(y => y.Message.VotingSessionId.CorrelationId));
        Event(() => RemoveVoteEvent, x => x.CorrelateById(y => y.Message.VotingSessionId.CorrelationId));
        Event(() => ConcludeVoting, x => x.CorrelateById(y => y.Message.VotingSessionId.CorrelationId));
        Event(() => GetVotingStatusEvent, x => x.CorrelateById(y => y.Message.VotingSessionId.CorrelationId));
        Event(() => GetMovieListEvent, x => x.CorrelateById(y => y.Message.VotingSessionId.CorrelationId));
        Event(() => GetNominationsEvent, x => x.CorrelateById(y => y.Message.VotingSessionId.CorrelationId));
        Event(() => ResultsConfirmedEvent, x => x.CorrelateById(y => y.Message.VotingSessionId.CorrelationId));
        Event(() => ResumeVotingEvent, x => x.CorrelateById(y => y.Message.VotingSessionId.CorrelationId));
        Event(() => Error, x => x.CorrelateById(y => y.Message.VotingSessionId.CorrelationId));

        Initially(
            When(StartVotingEvent)
                .Activity(x => x.OfType<CreateVotingSessionEntryActivity>())
                .Then(ctx =>
                {
                    this.logger.LogInformation("Voting is starting...");
                    var movies = ctx.Message.Movies.Select(x => new EmbeddedMovieWithVotes { Movie = x, Votes = [] }).ToArray();
                    ctx.Saga.Movies = movies;
                    ctx.Saga.Nominations = ctx.Message.NominationsData;
                    ctx.Saga.TenantId = ctx.Message.TenantId.Id;
                    ctx.Saga.Created = this.dateTimeProvider.Now;
                })
                .ThenAsync(async ctx =>
                {
                    foreach (var nominationData in ctx.Message.NominationsData)
                    {
                        await ctx.Publish(new NominationAddedEvent(ctx.Message.VotingSessionId, nominationData), ctx.CancellationToken);
                    }
                })
                .Publish(ctx => new VotingStartingEvent(new VotingSessionId(ctx.Saga.CorrelationId)))
                .TransitionTo(WaitingForNominations));

        During(WaitingForNominations, When(AddMovieEvent).Activity(x => x.OfType<AddMovieActivity>()));

        During([WaitingForNominations, NominationsConcluded],
            When(RemoveMovieEvent)
                 .Activity(x => x.OfType<RemoveMovieActivity>())
        );

        During([WaitingForNominations, NominationsConcluded, ExtraVoting],
            When(AddVoteEvent)
                .Then(ctx => this.logger.LogInformation("Adding a vote..."))
                .Then(ctx =>
                {
                    var movie = GetMovie(ctx.Saga, ctx.Message.Movie.id);
                    var messageUser = ctx.Message.User;

                    if (movie.Votes.Any(x => x.User.id == messageUser.Id))
                        return;

                    var user = new EmbeddedUser { id = messageUser.Id, Name = messageUser.Name, TenantId = messageUser.Tenant.Id };
                    movie.Votes = movie.Votes.Concat([new Vote { User = user, VoteType = ctx.Message.VoteType }]);
                    movie.VotingScore += ctx.Message.VoteType.GetVoteCount();
                })
                .Publish(ctx => new VoteAddedEvent(ctx.Message.VotingSessionId, ctx.Message.Movie, ctx.Message.User)));

        During([WaitingForNominations, NominationsConcluded, ExtraVoting],
            When(RemoveVoteEvent)
                .Then(ctx => this.logger.LogInformation("Removing a vote..."))
                .Then(ctx =>
                {
                    var movie = GetMovie(ctx.Saga, ctx.Message.Movie.id);
                    var messageUser = ctx.Message.User;

                    if (movie.Votes.All(x => x.User.id != messageUser.Id))
                        return;

                    var voteToRemove = movie.Votes.Single(x => x.User.id == messageUser.Id);
                    movie.Votes = movie.Votes.Except([voteToRemove]).ToArray();
                    movie.VotingScore -= voteToRemove.VoteType.GetVoteCount();
                })
                .Publish(ctx => new VoteAddedEvent(ctx.Message.VotingSessionId, ctx.Message.Movie, ctx.Message.User)));

        During([NominationsConcluded, ExtraVoting], When(ConcludeVoting).Activity(x => x.OfType<ConcludeVotingActivity>()));

        During(CalculatingExtraResults, When(ResumeVotingEvent).TransitionTo(ExtraVoting));
        During(CalculatingExtraResults, When(ResultsConfirmedEvent).Finalize());

        During([CalculatingResults, ExtraVoting], When(ResumeVotingEvent).TransitionTo(NominationsConcluded));
        During(CalculatingResults, When(ResultsConfirmedEvent).Finalize());
        During([ExtraVoting, CalculatingResults, CalculatingExtraResults, Final], When(GetNominationsEvent).Respond(ctx => new CurrentNominationsResponse { Nominations = [], CorrelationId = ctx.Saga.CorrelationId }));
        During([CalculatingResults, NominationsConcluded], When(Error)
            .Then(ctx => ctx.Saga.Error = new ErrorData { ErrorMessage = ctx.Message.Message, CallStack = ctx.Message.CallStack })
            .TransitionTo(NominationsConcluded));

        During([WaitingForNominations, NominationsConcluded, CalculatingResults, Final],
            When(GetMovieListEvent)
                .Respond(ctx => new CurrentVotingListResponse { Movies = GetMovies(ctx.Saga).Cast<IReadOnlyEmbeddedMovieWithVotes>().ToArray(), IsExtraVoting = false }));

        During([CalculatingExtraResults, ExtraVoting],
            When(GetMovieListEvent)
                .Respond(ctx => new CurrentVotingListResponse { Movies = GetMovies(ctx.Saga).Cast<IReadOnlyEmbeddedMovieWithVotes>().ToArray(), IsExtraVoting = true }));

        During([WaitingForNominations, NominationsConcluded],
            When(GetNominationsEvent)
                .Respond(ctx => new CurrentNominationsResponse { Nominations = ctx.Saga.Nominations.ToArray(), CorrelationId = ctx.Saga.CorrelationId }));

        During([WaitingForNominations, NominationsConcluded, ExtraVoting, CalculatingResults, CalculatingExtraResults, Final], When(GetVotingStatusEvent)
            .Respond(ctx => new CurrentVotingStatusResponse { State = ctx.Saga.CurrentState! }));
    }

    // Commands
    public Event<StartVotingEvent> StartVotingEvent { get; private set; } = null!;

    public Event<AddNominationsEvent> InitNominationsEvent { get; private set; } = null!;

    public Event<AddMovieEvent> AddMovieEvent { get; private set; } = null!;
    public Event<MovieAddedEvent> MovieAddedEvent { get; private set; } = null!;

    public Event<RemoveMovieEvent> RemoveMovieEvent { get; private set; } = null!;

    public Event<AddVoteEvent> AddVoteEvent { get; private set; } = null!;

    public Event<RemoveVoteEvent> RemoveVoteEvent { get; private set; } = null!;

    public Event<ConcludeVotingEvent> ConcludeVoting { get; private set; } = null!;
    public Event<ResumeVotingEvent> ResumeVotingEvent { get; private set; } = null!;

    // Events
    public Event<GetVotingStatusEvent> GetVotingStatusEvent { get; private set; } = null!;
    public Event<MoviesListRequestedEvent> GetMovieListEvent { get; private set; } = null!;
    public Event<NominationsRequestedEvent> GetNominationsEvent { get; private set; } = null!;
    public Event<ResultsConfirmedEvent> ResultsConfirmedEvent { get; private set; } = null!;
    public Event<ErrorEvent> Error { get; private set; } = null!;

    // States
    public State WaitingForNominations { get; private set; } = null!;

    public State NominationsConcluded { get; private set; } = null!;

    public State CalculatingResults { get; private set; } = null!;
    public State CalculatingExtraResults { get; private set; } = null!;
    public State ExtraVoting { get; private set; } = null!;

    // helpers
    private static EmbeddedMovieWithVotes GetMovie(VotingStateInstance stateInstance, string movieId)
    {
        var moviesToPick = GetMovies(stateInstance);
        return moviesToPick!.Single(x => x.Movie.id == movieId);
    }

    private static IEnumerable<EmbeddedMovieWithVotes> GetMovies(VotingStateInstance stateInstance)
    {
        string[] extraVotingStates = [nameof(ExtraVoting), nameof(CalculatingExtraResults)];
        return extraVotingStates.Contains(stateInstance.CurrentState, StringComparer.OrdinalIgnoreCase) 
            ? stateInstance.ExtraVotingMovies! 
            : stateInstance.Movies;
    }
}
