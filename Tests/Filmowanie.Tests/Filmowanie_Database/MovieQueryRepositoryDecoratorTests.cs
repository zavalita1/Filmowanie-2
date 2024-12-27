using System.Linq.Expressions;
using Filmowanie.Abstractions;
using Filmowanie.Database.Decorators;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using FluentAssertions;

namespace Filmowanie.Tests.Filmowanie_Database;

public sealed class MovieQueryRepositoryDecoratorTests
{
    private readonly MovieQueryRepositoryDecorator _decorator;

    public MovieQueryRepositoryDecoratorTests()
    {
        var decorated = new UsersRepositoryForTests();
        _decorator = new MovieQueryRepositoryDecorator(decorated);
    }

    [Fact]
    public async Task GetMoviesAsync_IncludeTenantCondition()
    {
        // Arrange
        // Act
        var result = await _decorator.GetMoviesAsync(x => x.Name == "Testyy", new TenantId(7), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.Single().Description.Should().Be("this one");
    }

    [Fact]
    public async Task GetMoviesThatCanBeNominatedAgainEntityAsync_IncludeTenantCondition()
    {
        // Arrange
        // Act
        var result = await _decorator.GetMoviesThatCanBeNominatedAgainEntityAsync(x => x.Movie.Name == "Testyy", new TenantId(3), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.Single().Movie.MovieCreationYear.Should().Be(2137);
    }

    [Fact]
    public async Task GetMoviesNominatedAgainEntityAsync_IncludeTenantCondition()
    {
        // Arrange
        // Act
        var result = await _decorator.GetMoviesNominatedAgainEntityAsync(x => x.Movie.Name == "Testyy2", new TenantId(3), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.Single().Movie.MovieCreationYear.Should().Be(2137);
    }

    private sealed class UsersRepositoryForTests : IMovieQueryRepository
    {
        public readonly MovieEntity[] MockMovies =
        [
            new() { TenantId = 21 },
            new() { TenantId = 3, Name = "Testyy"},
            new() { TenantId = 7, Name = "Testyy 2"},
            new() { TenantId = 7, Name = "Testyy", Description = "this one"},
        ];

        public readonly CanNominateMovieAgainEvent[] CanCanNominateMovieAgainEvents =
        [
            new() { TenantId = 21, Movie = new EmbeddedMovie { Name = "Testyy1"}},
            new() { TenantId = 3, Type = "test", Movie = new EmbeddedMovie { Name = "Testyy", MovieCreationYear = 2137}},
            new() { TenantId = 3, Movie = new EmbeddedMovie { Name = "Testyy2"}},
            new() { TenantId = 7, Type = "test", Movie = new EmbeddedMovie { Name = "Testyy"}},
        ];

        public readonly NominatedMovieAgainEvent[] NominatedMovieAgainEvents =
        [
            new() { TenantId = 21, Movie = new EmbeddedMovie { Name = "Testyy1"}},
            new() { TenantId = 3, Movie = new EmbeddedMovie { Name = "Testyy2"}},
            new() { TenantId = 3, Movie = new EmbeddedMovie { Name = "Testyy2b"}},
            new() { TenantId = 7, Movie = new EmbeddedMovie { Name = "Testyy3"}},
        ];

        public Task<IReadOnlyMovieEntity[]> GetMoviesAsync(Expression<Func<IReadOnlyMovieEntity, bool>> predicate, TenantId tenant, CancellationToken cancellationToken)
        {
            var func = predicate.Compile();
            var result = MockMovies.Where(func).ToArray();
            return Task.FromResult(result);
        }

        public Task<IReadOnlyCanNominateMovieAgainEvent[]> GetMoviesThatCanBeNominatedAgainEntityAsync(Expression<Func<IReadOnlyCanNominateMovieAgainEvent, bool>> predicate, TenantId tenant, CancellationToken cancellationToken)
        {
            var func = predicate.Compile();
            var result = CanCanNominateMovieAgainEvents.Where(func).ToArray();
            return Task.FromResult(result);
        }

        public Task<IReadOnlyNominatedMovieAgainEvent[]> GetMoviesNominatedAgainEntityAsync(Expression<Func<IReadOnlyNominatedMovieAgainEvent, bool>> predicate, TenantId tenant, CancellationToken cancellationToken)
        {
            var func = predicate.Compile();
            var result = NominatedMovieAgainEvents.Where(func).ToArray();
            return Task.FromResult(result);
        }
    }
}