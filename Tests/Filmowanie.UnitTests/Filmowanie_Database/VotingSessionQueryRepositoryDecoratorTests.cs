using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Decorators;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using NSubstitute;
using System.Linq.Expressions;
using Filmowanie.Database.Entities.Voting;
using FluentAssertions;
using Filmowanie.Abstractions.Enums;

namespace Filmowanie.UnitTests.Filmowanie_Database;

public sealed class VotingSessionQueryRepositoryDecoratorTests
{
    private readonly VotingSessionQueryRepositoryDecorator _sut;
    private readonly IVotingSessionQueryRepository _repository;

    public VotingSessionQueryRepositoryDecoratorTests()
    {
        _repository = Substitute.For<IVotingSessionQueryRepository>();
        var userAccessor = Substitute.For<ICurrentUserAccessor>();
        userAccessor.GetDomainUser(VoidResult.Void).Returns(new DomainUser("whatever", "whatever", false, false, new TenantId(2137), new DateTime(2010, 04, 10), Gender.Unspecified).AsMaybe());
        _sut = new VotingSessionQueryRepositoryDecorator(_repository, userAccessor);
    }

    [Fact]
    public async Task Get_ModifiesPredicateAccordingly()
    {
        // Arrange
        var stubbedDbEntities = new[]
        {
            new VotingResult { id = "loo", Winner = new () { Movie = new() { Name = "double loo", MovieCreationYear = 3333 }}, TenantId = 213 },
            new VotingResult { id = "loo", Winner = new () { Movie = new() { Name = "double loo", MovieCreationYear = 1111 }}, TenantId = 2137 },
            new VotingResult { id = "loo2", Winner = new () { Movie = new() { id = "double loo", MovieCreationYear = 5555 }}, TenantId = 2137 },
            new VotingResult { id = "loo2", Winner = new () { Movie = new() { Name = "double loo", MovieCreationYear = 4444 }}, TenantId = 2137 },
        };
        _repository.Get(null!, CancellationToken.None)
            .ReturnsForAnyArgs(ci => stubbedDbEntities.Single(x => ci.ArgAt<Expression<Func<IReadOnlyVotingResult, bool>>>(0).Compile().Invoke(x)));

        // Act
        var result = await _sut.Get(x => x.id == "loo" && x.Winner!.Movie.Name == "double loo", CancellationToken.None);


        // Assert
        result.Should().NotBeNull();
        result!.Winner!.Movie.MovieCreationYear.Should().Be(1111);
    }

    [Fact]
    public async Task GetVotingResultAsync_ModifiesPredicateAccordingly()
    {
        // Arrange
        var stubbedDbEntities = new[]
        {
            new VotingResult { id = "loo", Winner = new () { Movie = new() { Name = "double loo", MovieCreationYear = 3333 }}, TenantId = 213 },
            new VotingResult { id = "loo", Winner = new () { Movie = new() { Name = "double loo", MovieCreationYear = 1111 }}, TenantId = 2137 },
            new VotingResult { id = "loo2", Winner = new () { Movie = new() { id = "double loo", MovieCreationYear = 5555 }}, TenantId = 2137 },
            new VotingResult { id = "loo2", Winner = new () { Movie = new() { Name = "double loo", MovieCreationYear = 4444 }}, TenantId = 2137 },
            new VotingResult { id = "loo", Winner = new () { Movie = new() { Name = "double loo", MovieCreationYear = 6666 }}, TenantId = 2137 },
        };
        _repository.GetVotingResultAsync(null!, null!, 0, CancellationToken.None)
            .ReturnsForAnyArgs(ci => stubbedDbEntities.Where(x => ci.ArgAt<Expression<Func<IReadOnlyVotingResult, bool>>>(0).Compile().Invoke(x)).ToArray());

        // Act
        var result = (await _sut.GetVotingResultAsync(x => x.id == "loo" && x.Winner!.Movie.Name == "double loo", x => x, 10, CancellationToken.None)).ToArray();


        // Assert
        result.Should().HaveCount(2);
        result.First().Winner!.Movie.MovieCreationYear.Should().Be(1111);
        result.Last().Winner!.Movie.MovieCreationYear.Should().Be(6666);
    }

    [Fact]
    public async Task GetMoviesAsync_ModifiesPredicateAccordingly()
    {
        // Arrange
        var stubbedDbEntities = new[]
        {
            new VotingResult { id = "loo", Winner = new () { Movie = new() { Name = "double loo", MovieCreationYear = 3333 }}, TenantId = 213 },
            new VotingResult { id = "loo", Winner = new () { Movie = new() { Name = "double loo", MovieCreationYear = 1111 }}, TenantId = 2137 },
            new VotingResult { id = "loo2", Winner = new () { Movie = new() { id = "double loo", MovieCreationYear = 5555 }}, TenantId = 2137 },
            new VotingResult { id = "loo2", Winner = new () { Movie = new() { Name = "double loo", MovieCreationYear = 4444 }}, TenantId = 2137 },
            new VotingResult { id = "loo", Winner = new () { Movie = new() { Name = "double loo", MovieCreationYear = 6666 }}, TenantId = 2137 },
        };
        _repository.GetAll(null!, CancellationToken.None)
            .ReturnsForAnyArgs(ci => stubbedDbEntities.Where(x => ci.ArgAt<Expression<Func<IReadOnlyVotingResult, bool>>>(0).Compile().Invoke(x)).ToArray<IReadOnlyVotingResult>());

        // Act
        var result = (await _sut.GetAll(x => x.id == "loo" && x.Winner!.Movie.Name == "double loo", CancellationToken.None)).ToArray();

        // Assert
        result.Should().HaveCount(2);
        result.First().Winner!.Movie.MovieCreationYear.Should().Be(1111);
        result.Last().Winner!.Movie.MovieCreationYear.Should().Be(6666);
    }
}