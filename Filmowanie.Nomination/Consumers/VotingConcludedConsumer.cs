using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using InvalidOperationException = System.InvalidOperationException;

namespace Filmowanie.Nomination.Consumers;

public sealed class VotingConcludedConsumer : IConsumer<VotingConcludedEvent>, IConsumer<Fault<VotingConcludedEvent>>
{
    private readonly ILogger<VotingConcludedConsumer> _logger;
    private readonly IVotingSessionQueryRepository _votesRepository;
    private readonly IMovieQueryRepository _movieQueryRepository;
    private readonly IMovieCommandRepository _movieCommandRepository;
    private const int TimeWindow = 10;

    public VotingConcludedConsumer(ILogger<VotingConcludedConsumer> logger, IVotingSessionQueryRepository votesRepository, IMovieQueryRepository movieQueryRepository, IMovieCommandRepository movieCommandRepository)
    {
        _logger = logger;
        _votesRepository = votesRepository;
        _movieQueryRepository = movieQueryRepository;
        _movieCommandRepository = movieCommandRepository;
    }

    public Task Consume(ConsumeContext<Fault<VotingConcludedEvent>> context)
    {
        var message = string.Join(",", context.Message.Exceptions.Select(x => x.Message));
        _logger.LogError($"ERROR WHEN CONSIDERING MOVIES THAT CAN BE NOMINATED AGAIN! {message}.");
        return Task.CompletedTask;
    }

    public async Task Consume(ConsumeContext<VotingConcludedEvent> context)
    {
        _logger.LogInformation($"Consuming {nameof(VotingConcludedEvent)}...");
        
        var message = context.Message;
        var votingResultOfInterest = (await _votesRepository.Get(x => x.Concluded != null && x.TenantId == message.Tenant.Id, x => x.Concluded!, -1 * TimeWindow, context.CancellationToken)).Last();

        var moviesThatCanBeNominatedAgain = await _movieQueryRepository.GetMoviesThatCanBeNominatedAgainEntityAsync(x => x.TenantId == message.Tenant.Id, context.CancellationToken);

        if (moviesThatCanBeNominatedAgain == null)
            throw new InvalidOperationException("No entry for movies that can be nominated again in db!");

        var newMoviesToAdd = votingResultOfInterest.MoviesGoingByeBye.Select(x => new EmbeddedMovie { id = x.id, MovieCreationYear = x.MovieCreationYear, Name = x.Name });
        var movies = moviesThatCanBeNominatedAgain.Movies.Concat(newMoviesToAdd);

        await _movieCommandRepository.UpdateMoviesThatCanBeNominatedAgainEntityAsync(moviesThatCanBeNominatedAgain.Id, movies, context.CancellationToken);

        _logger.LogInformation($"Consumed {nameof(VotingConcludedEvent)} event.");
    }
}