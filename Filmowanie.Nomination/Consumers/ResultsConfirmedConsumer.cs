using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Nomination.Consumers;

public sealed class ResultsConfirmedConsumer : IConsumer<ResultsConfirmedEvent>, IConsumer<Fault<ResultsConfirmedEvent>>
{
    private readonly ILogger<ResultsConfirmedConsumer> logger;
    private readonly IRepositoryInUserlessContextProvider repositoriesProvider;
    private readonly IMovieCommandRepository movieCommandRepository;
    private readonly IGuidProvider guidProvider;
    private readonly IDateTimeProvider dateTimeProvider;
    private const int TimeWindow = 10;

    public ResultsConfirmedConsumer(ILogger<ResultsConfirmedConsumer> logger, IRepositoryInUserlessContextProvider repositoryProvider, IMovieCommandRepository movieCommandRepository, IGuidProvider guidProvider, IDateTimeProvider dateTimeProvider)
    {
        this.logger = logger;
        this.repositoriesProvider = repositoryProvider;
        this.movieCommandRepository = movieCommandRepository;
        this.guidProvider = guidProvider;
        this.dateTimeProvider = dateTimeProvider;
    }

    public Task Consume(ConsumeContext<Fault<ResultsConfirmedEvent>> context)
    {
        var message = context.Message.Exceptions.Select(x => x.Message).JoinStrings();
        this.logger.LogError($"ERROR WHEN CONSIDERING MOVIES THAT CAN BE NOMINATED AGAIN! {message}.");
        return Task.CompletedTask;
    }

    public async Task Consume(ConsumeContext<ResultsConfirmedEvent> context)
    {
        this.logger.LogInformation($"Consuming {nameof(ResultsConfirmedEvent)}...");

        try
        {
            var message = context.Message;
            var repo = this.repositoriesProvider.GetVotingResultsRepository(message.Tenant);
            var readOnlyVotingResults = await repo.GetLastNVotingResultsAsync(TimeWindow, context.CancellationToken);
            var lastVotingResult = readOnlyVotingResults.RequireResult().Last();

            var newMoviesToAdd = lastVotingResult.MoviesGoingByeBye.Select(x => GetReadOnlyCanNominateMovieAgainEvent(x, message));
            await this.movieCommandRepository.InsertCanBeNominatedAgainAsync(newMoviesToAdd, context.CancellationToken);

            var winnerId = new MovieId(lastVotingResult.Winner!.Movie.id);
            await this.movieCommandRepository.DeleteEventsForMovieAsync(winnerId, context.CancellationToken);
        }
        catch (Exception ex)
        {
            var msg = "Error occurred during concluding the voting...";
            this.logger.LogError(ex, msg);
            var errorEvent = new ErrorEvent(context.Message.VotingSessionId, msg, ex.StackTrace ?? "Unknown");
            await context.Publish(errorEvent);
        }

        this.logger.LogInformation($"Consumed {nameof(VotingConcludedEvent)} event.");
    }

    private IReadOnlyCanNominateMovieAgainEvent GetReadOnlyCanNominateMovieAgainEvent(IReadOnlyEmbeddedMovie x, ResultsConfirmedEvent message)
    {
        var tenantId = message.Tenant.Id;
        var now = this.dateTimeProvider.Now;
        var guid = this.guidProvider.NewGuid();
        var id = $"nominate-again-event-{guid}";
        return new CanNominateMovieAgainEventRecord(x, id, now, tenantId);
    }
}