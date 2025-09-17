using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Filmowanie.Abstractions;
using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Database.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Filmowanie.Nomination.Consumers;
using FluentAssertions;
using MassTransit.Events;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting.Events;

namespace Filmowanie.Tests.Filmowanie_Nomination;

public class VotingConcludedConsumerTests
{
    private readonly ILogger<ResultsConfirmedConsumer> _logger;
    private readonly IVotingResultsRepository _votesRepository;
    private readonly IMovieCommandRepository _movieCommandRepository;
    private readonly IGuidProvider _guidProvider;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ResultsConfirmedConsumer _consumer;

    private readonly IFixture _fixture = new Fixture();

    public VotingConcludedConsumerTests()
    {
        _logger = Substitute.For<ILogger<ResultsConfirmedConsumer>>();
        _votesRepository = Substitute.For<IVotingResultsRepository>();
        _movieCommandRepository = Substitute.For<IMovieCommandRepository>();
        _guidProvider = Substitute.For<IGuidProvider>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _consumer = new VotingConcludedConsumer(_logger, _votesRepository, _movieCommandRepository, _guidProvider, _dateTimeProvider);

        _fixture.Customize(new AutoNSubstituteCustomization());
    }

    [Fact]
    public async Task Consume_ShouldLogError_WhenFaultEventIsConsumed()
    {
        // Arrange
        var context = Substitute.For<ConsumeContext<Fault<VotingConcludedEvent>>>();
        context.Message.Exceptions.Returns([new FaultExceptionInfo(new Exception("Test exception"))]);

        // Act
        await _consumer.Consume(context);

        // Assert
        _logger.Received(1).LogError("ERROR WHEN CONSIDERING MOVIES THAT CAN BE NOMINATED AGAIN! Test exception.");
    }

    [Fact]
    public async Task Consume_ShouldProcessVotingConcludedEventCorrectly()
    {
        // Arrange
        var votingResult1 = Substitute.For<IReadOnlyVotingResult>();
        var votingResult2 = Substitute.For<IReadOnlyVotingResult>();
        var movieGoingByeBye1 = Substitute.For<IReadOnlyEmbeddedMovie>();
        movieGoingByeBye1.Name.Returns("The Matrix");
        var movieGoingByeBye2 = Substitute.For<IReadOnlyEmbeddedMovie>();
        movieGoingByeBye2.Name.Returns("The Godfather");
        votingResult2.MoviesGoingByeBye.Returns([movieGoingByeBye1, movieGoingByeBye2]);

        var context = Substitute.For<ConsumeContext<VotingConcludedEvent>>();
        var @event = _fixture.Build<VotingConcludedEvent>().With(x => x.Tenant, new TenantId(2137)).Create();
        context.Message.Returns(@event);
        _votesRepository.GetLastNVotingResultsAsync(default, default)
            .ReturnsForAnyArgs(Task.FromResult(new Maybe<IEnumerable<IReadOnlyVotingResult>>([votingResult1, votingResult2], null)));

        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        _guidProvider.NewGuid().Returns(guid1, guid2);
        var now1 = new DateTime(2010, 04, 10);
        var now2 = DateTime.UtcNow;
        _dateTimeProvider.Now.Returns(now1, now2);

        IEnumerable<IReadOnlyCanNominateMovieAgainEvent> capturedInput = null!;
        _movieCommandRepository.InsertCanBeNominatedAgainAsync(default!, default).ReturnsForAnyArgs(Task.CompletedTask)
            .AndDoes(x => capturedInput = x.ArgAt<IEnumerable<IReadOnlyCanNominateMovieAgainEvent>>(0));

        var expectedInput = new[]
        {
            new CanNominateMovieAgainEventRecord(movieGoingByeBye1, $"nominate-again-event-{guid1}", now1, 2137),
            new CanNominateMovieAgainEventRecord(movieGoingByeBye2, $"nominate-again-event-{guid2}", now2, 2137),
        };

        // Act
        await _consumer.Consume(context);

        // Assert
        capturedInput.Should().BeEquivalentTo(expectedInput);
    }
}