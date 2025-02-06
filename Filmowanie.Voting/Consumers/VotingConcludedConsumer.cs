using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Extensions;
using Filmowanie.Database.Interfaces;
using Filmowanie.Voting.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Consumers;

internal sealed class VotingConcludedConsumer : IConsumer<VotingConcludedEvent>, IConsumer<Fault<VotingConcludedEvent>>
{
    private readonly ILogger<VotingConcludedConsumer> _logger;
    private readonly IVotingSessionCommandRepository _votingSessionCommandRepository;
    private readonly IVotingSessionQueryRepository _votingSessionQueryRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPickUserToNominateContextRetriever _pickUserToNominateContextRetriever;
    private readonly IVotingResultsRetriever _votingResultsRetriever;
    private readonly INominationsRetriever _nominationsRetriever;

    private const int DecidersTimeWindow = 10;

    public VotingConcludedConsumer(ILogger<VotingConcludedConsumer> logger, IVotingSessionCommandRepository votingSessionCommandRepository, IDateTimeProvider dateTimeProvider, IVotingSessionQueryRepository votingSessionQueryRepository, IPickUserToNominateContextRetriever pickUserToNominateContextRetriever, IVotingResultsRetriever votingResultsRetriever, INominationsRetriever nominationsRetriever)
    {
        _logger = logger;
        _votingSessionCommandRepository = votingSessionCommandRepository;
        _dateTimeProvider = dateTimeProvider;
        _votingSessionQueryRepository = votingSessionQueryRepository;
        _pickUserToNominateContextRetriever = pickUserToNominateContextRetriever;
        _votingResultsRetriever = votingResultsRetriever;
        _nominationsRetriever = nominationsRetriever;
    }

    public Task Consume(ConsumeContext<Fault<VotingConcludedEvent>> context)
    {
        var message = string.Join(",", context.Message.Exceptions.Select(x => x.Message));
        var callStacks = string.Join(";;;;;;;;;;;;", context.Message.Exceptions.Select(x => x.StackTrace));
        return context.Publish(new ErrorEvent(context.Message.Message.CorrelationId, message, callStacks));
    }

    public async Task Consume(ConsumeContext<VotingConcludedEvent> context)
    {
        _logger.LogInformation($"Consuming {nameof(VotingConcludedEvent)}...");
        var now = _dateTimeProvider.Now;
        var message = context.Message;

        var lastVotingResults = (await _votingSessionQueryRepository.Get(x => x.Concluded != null, message.Tenant, x => x.Concluded!, -1 * DecidersTimeWindow, context.CancellationToken)).ToArray();
        var readonlyCurrentVotingResult = (await _votingSessionQueryRepository.Get(x => x.Concluded == null, message.Tenant, context.CancellationToken))!;
        var votingResults = _votingResultsRetriever.GetVotingResults(message.MoviesWithVotes, lastVotingResults.FirstOrDefault()); // null only if it's initial voting

        var moviesAdded = GetMoviesAddedDuringSession(message);

        var assignNominationsUserContexts = _pickUserToNominateContextRetriever.GetPickUserToNominateContexts(lastVotingResults, moviesAdded, message);
        var nominations = _nominationsRetriever.GetNominations(assignNominationsUserContexts, message, votingResults);

        await _votingSessionCommandRepository.UpdateAsync(readonlyCurrentVotingResult.id, votingResults.Movies, nominations, now, moviesAdded, votingResults.Winner, context.CancellationToken);
        await context.Publish(new ResultsCalculated(message.CorrelationId));

        _logger.LogInformation($"Consumed {nameof(VotingConcludedEvent)} event.");
    }

    private static EmbeddedMovieWithNominationContext[] GetMoviesAddedDuringSession(VotingConcludedEvent message)
    {
        return message.NominationsData.Join(message.MoviesWithVotes, x => x.MovieId, x => x.Movie.id, (x, y) => new EmbeddedMovieWithNominationContext
        {
            Movie = y.Movie.AsMutable(),
            NominatedBy = new EmbeddedUser { id = x.User.Id, Name = x.User.DisplayName, TenantId = message.Tenant.Id },
            NominationConcluded = x.Concluded!.Value,
            NominationStarted = message.VotingStarted
        }).ToArray();
    }
}