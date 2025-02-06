using System.Linq.Expressions;
using AutoFixture;
using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.Consumers;
using Filmowanie.Voting.DomainModels;
using Filmowanie.Voting.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Filmowanie.Tests.Filmowanie_Voting;

public class VotingConcludedConsumerTests
{
    private readonly VotingConcludedConsumer _sut;
    private readonly IVotingSessionCommandRepository _votingSessionCommandRepository;
    private readonly VotingSessionRepository _votingSessionQueryRepository;
    private readonly IPickUserToNominateContextRetriever _pickUserToNominateContextRetriever;
    private readonly IVotingResultsRetriever _votingResultsRetriever;
    private readonly INominationsRetriever _nominationsRetriever;

    private readonly IFixture _fixture = new Fixture();

    public VotingConcludedConsumerTests()
    {
        var logger = Substitute.For<ILogger<VotingConcludedConsumer>>();
        _votingSessionCommandRepository = Substitute.For<IVotingSessionCommandRepository>();
        var dateProvider = Substitute.For<IDateTimeProvider>();
        dateProvider.Now.Returns(new DateTime(2010, 04, 10));
        _votingSessionQueryRepository = new VotingSessionRepository();
        _votingResultsRetriever = Substitute.For<IVotingResultsRetriever>();
        _pickUserToNominateContextRetriever = Substitute.For<IPickUserToNominateContextRetriever>();
        _nominationsRetriever = Substitute.For<INominationsRetriever>();
        _sut = new VotingConcludedConsumer(logger, _votingSessionCommandRepository, dateProvider, _votingSessionQueryRepository, _pickUserToNominateContextRetriever, _votingResultsRetriever, _nominationsRetriever);
    }

    [Fact]
    public async Task Consume_PassesProperArguments()
    {
        // Arrange
        var consumeContext = Substitute.For<ConsumeContext<VotingConcludedEvent>>();
        var movies = new[]
        {
            new EmbeddedMovieWithVotes { Movie = new EmbeddedMovie() },
            new EmbeddedMovieWithVotes { Movie = new EmbeddedMovie { id = "movie2", Name = "movie name 2"}},
            new EmbeddedMovieWithVotes { Movie = new EmbeddedMovie { id = "movie1", Name = "movie name 1"}},
        };
        var nominations = new[]
        {
            new NominationData { MovieId = "movie1", Concluded = new DateTime(2010, 05, 11),  User = new NominationDataEmbeddedUser { Id = "user id 1", DisplayName = "user name 1"}},
            new NominationData { MovieId = "movie2", Concluded = new DateTime(2001, 01, 01),  User = new NominationDataEmbeddedUser { Id = "user id 2", DisplayName = "user name 2"}},
        };
        var tenantId = new TenantId(21);
        consumeContext.Message.Returns(new VotingConcludedEvent(Guid.Empty, tenantId, movies, nominations, new DateTime(2005, 04, 02)));
        var votingResults = new VotingResults(_fixture.CreateMany<EmbeddedMovie>().ToArray(), _fixture.CreateMany<EmbeddedMovieWithVotes>().ToArray(), _fixture.Create<EmbeddedMovie>());
        _votingResultsRetriever.GetVotingResults(consumeContext.Message.MoviesWithVotes, _votingSessionQueryRepository.Results[2]).Returns(votingResults);
        var contexts = new Dictionary<IReadOnlyEmbeddedUser, PickUserToNominateContext>();
        _pickUserToNominateContextRetriever.GetPickUserToNominateContexts(default!, default!, default!).ReturnsForAnyArgs(contexts);

        var nominationsAwards = _fixture.CreateMany<EmbeddedUserWithNominationAward>().ToList();
        _nominationsRetriever.GetNominations(contexts, consumeContext.Message, votingResults).Returns(nominationsAwards);

        // Act
        await _sut.Consume(consumeContext);

        // Assert
        _votingResultsRetriever.Received(1).GetVotingResults(consumeContext.Message.MoviesWithVotes, _votingSessionQueryRepository.Results[2]);
        await _votingSessionCommandRepository.Received(1).UpdateAsync("this id!", votingResults.Movies, nominationsAwards, new DateTime(2010, 04, 10), 
            Arg.Is<EmbeddedMovieWithNominationContext[]>(x => x.ElementAt(0).Movie.Name == "movie name 1" && x.ElementAt(1).Movie.Name == "movie name 2" ),
            votingResults.Winner, consumeContext.CancellationToken);
    }

    private sealed class VotingSessionRepository : IVotingSessionQueryRepository
    {
        public readonly IReadOnlyVotingResult[] Results =
        [
            new VotingResult { Concluded = new DateTime(2010, 04, 10) },
            new VotingResult { Concluded = null, id = "this id!"},
            new VotingResult { Concluded = new DateTime(2010, 04, 09) },
            new VotingResult { Concluded = new DateTime(2010, 04, 11) },
            new VotingResult { Concluded = new DateTime(2010, 04, 10) },
            new VotingResult { Concluded = new DateTime(2010, 04, 10) },
            new VotingResult { Concluded = new DateTime(2010, 04, 10) },
            new VotingResult { Concluded = new DateTime(2010, 04, 10) },
            new VotingResult { Concluded = new DateTime(2010, 04, 10) },
            new VotingResult { Concluded = new DateTime(2011, 04, 10) },
            new VotingResult { Concluded = new DateTime(2011, 04, 10) },
            new VotingResult { Concluded = new DateTime(2012, 04, 10) },
            new VotingResult { Concluded = new DateTime(2011, 04, 10) },
        ];

        public Task<IReadOnlyVotingResult?> Get(Expression<Func<IReadOnlyVotingResult, bool>> predicate, TenantId tenant, CancellationToken cancellationToken) => 
            Task.FromResult(Results.Single(predicate.Compile()));

        public Task<IEnumerable<IReadOnlyVotingResult>> Get(Expression<Func<IReadOnlyVotingResult, bool>> predicate, TenantId tenant, Expression<Func<IReadOnlyVotingResult, object>> sortBy, int take, CancellationToken cancellationToken) =>
            Task.FromResult(Results.Where(predicate.Compile()).OrderBy(sortBy.Compile()).Take(Math.Abs(take)));
       

        public Task<IEnumerable<T>> Get<T>(Expression<Func<IReadOnlyVotingResult, bool>> predicate, Expression<Func<IReadOnlyVotingResult, T>> selector, TenantId tenant, CancellationToken cancellationToken) where T : class 
            => throw new NotImplementedException();
    }
}