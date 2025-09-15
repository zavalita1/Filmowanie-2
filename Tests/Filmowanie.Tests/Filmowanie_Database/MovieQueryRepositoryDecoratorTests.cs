using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Decorators;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using NSubstitute;
using System.Linq.Expressions;
using Filmowanie.Database.Entities;
using FluentAssertions;

namespace Filmowanie.Tests.Filmowanie_Database;

public sealed class MovieQueryRepositoryDecoratorTests
{
    private readonly MovieQueryRepositoryDecorator _sut;
    private readonly IMovieQueryRepository _movieRepo;

    public MovieQueryRepositoryDecoratorTests()
    {
        _movieRepo = Substitute.For<IMovieQueryRepository>();
        var userAccessor = Substitute.For<ICurrentUserAccessor>();
        userAccessor.GetDomainUser(VoidResult.Void).Returns(new DomainUser("whatever", "whatever", false, false, new TenantId(2137), new DateTime(2010, 04, 10)).AsMaybe());
        _sut = new MovieQueryRepositoryDecorator(_movieRepo, userAccessor);
    }

    [Fact]
    public async Task GetMoviesNominatedAgainEventsAsync_ModifiesPredicateAccordingly()
    {
        // Arrange
        var stubbedDbEntities = new[]
        {
            new NominatedMovieEvent { id = "loo", Movie = new() { Name = "double loo", MovieCreationYear = 3333 }, TenantId = 213 },
            new NominatedMovieEvent { id = "loo", Movie = new() { Name = "double loo", MovieCreationYear = 1111 }, TenantId = 2137 },
            new NominatedMovieEvent { id = "loo2", Movie = new() { id = "double loo", MovieCreationYear = 5555 }, TenantId = 2137 },
            new NominatedMovieEvent { id = "loo2", Movie = new() { Name = "double loo", MovieCreationYear = 4444 }, TenantId = 2137 },
            new NominatedMovieEvent { id = "loo", Movie = new() { Name = "double loo", MovieCreationYear = 2222 }, TenantId = 2137 },
        };
        _movieRepo.GetMoviesNominatedAgainEventsAsync(default!, CancellationToken.None)
            .ReturnsForAnyArgs(ci => stubbedDbEntities.Where(x => ci.ArgAt<Expression<Func<IReadOnlyNominatedMovieEvent, bool>>>(0).Compile().Invoke(x)).ToArray<IReadOnlyNominatedMovieEvent>());

        // Act
        var result = await _sut.GetMoviesNominatedAgainEventsAsync(x => x.id == "loo" && x.Movie.Name == "double loo", CancellationToken.None);


        // Assert
        result.Should().HaveCount(2);
        result.First().Movie.MovieCreationYear.Should().Be(1111);
        result.Last().Movie.MovieCreationYear.Should().Be(2222);
    }

    [Fact]
    public async Task GetMoviesAsync_ModifiesPredicateAccordingly()
    {
        // Arrange
        var stubbedDbEntities = new[]
        {
            new MovieEntity { id = "loo", Name = "double loo", Description = "d1", TenantId = 213 },
            new MovieEntity { id = "loo", Name = "double loo", Description = "d2", TenantId = 2137 },
            new MovieEntity { id = "loo2", Name = "double loo", Description = "d3", TenantId = 2137 },
            new MovieEntity { id = "loo2", Name = "double loo 3", Description = "d4", TenantId = 2137 },
            new MovieEntity { id = "loo", Name = "double loo", Description = "d5", TenantId = 2137 },
        };
        _movieRepo.GetMoviesAsync(default!, CancellationToken.None)
            .ReturnsForAnyArgs(ci => stubbedDbEntities.Where(x => ci.ArgAt<Expression<Func<IReadOnlyMovieEntity, bool>>>(0).Compile().Invoke(x)).ToArray<IReadOnlyMovieEntity>());

        // Act
        var result = await _sut.GetMoviesAsync(x => x.id == "loo" && x.Name == "double loo", CancellationToken.None);


        // Assert
        result.Should().HaveCount(2);
        result.First().Description.Should().Be("d2");
        result.Last().Description.Should().Be("d5");
    }

    [Fact]
    public async Task GetMoviesThatCanBeNominatedAgainEventsAsync_ModifiesPredicateAccordingly()
    {
        // Arrange
        var stubbedDbEntities = new[]
        {
            new CanNominateMovieAgainEvent { id = "loo", Movie = new() { Name = "double loo", MovieCreationYear = 3333 }, TenantId = 213 },
            new CanNominateMovieAgainEvent { id = "loo", Movie = new() { Name = "double loo", MovieCreationYear = 1111 }, TenantId = 2137 },
            new CanNominateMovieAgainEvent { id = "loo2", Movie = new() { id = "double loo", MovieCreationYear = 5555 }, TenantId = 2137 },
            new CanNominateMovieAgainEvent { id = "loo2", Movie = new() { Name = "double loo", MovieCreationYear = 4444 }, TenantId = 2137 },
            new CanNominateMovieAgainEvent { id = "loo", Movie = new() { Name = "double loo", MovieCreationYear = 2222 }, TenantId = 2137 },
        };
        _movieRepo.GetMoviesThatCanBeNominatedAgainEventsAsync(default!, CancellationToken.None)
            .ReturnsForAnyArgs(ci => stubbedDbEntities.Where(x => ci.ArgAt<Expression<Func<IReadOnlyCanNominateMovieAgainEvent, bool>>>(0).Compile().Invoke(x)).ToArray<IReadOnlyCanNominateMovieAgainEvent>());

        // Act
        var result = await _sut.GetMoviesThatCanBeNominatedAgainEventsAsync(x => x.id == "loo" && x.Movie.Name == "double loo", CancellationToken.None);


        // Assert
        result.Should().HaveCount(2);
        result.First().Movie.MovieCreationYear.Should().Be(1111);
        result.Last().Movie.MovieCreationYear.Should().Be(2222);
    }
}