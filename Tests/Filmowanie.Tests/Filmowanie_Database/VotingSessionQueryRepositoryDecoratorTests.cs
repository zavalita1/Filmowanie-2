using System.Linq.Expressions;
using Filmowanie.Abstractions;
using Filmowanie.Database.Decorators;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using FluentAssertions;

namespace Filmowanie.Tests.Filmowanie_Database;

public sealed class VotingSessionQueryRepositoryDecoratorTests
{
    private readonly VotingSessionQueryRepositoryDecorator _decorator;

    public VotingSessionQueryRepositoryDecoratorTests()
    {
        var decorated = new UsersRepositoryForTests();
        _decorator = new VotingSessionQueryRepositoryDecorator(decorated);
    }

    [Fact]
    public async Task Get_IncludeTenantCondition()
    {
        // Arrange
        // Act
        var result = await _decorator.Get(x => x.Created == new DateTime(2010, 04, 10, 12, 1, 1), new TenantId(7), CancellationToken.None);

        // Assert
        result!.Winner.Should().NotBeNull();
    }

    [Fact]
    public async Task GetWithSelector_IncludeTenantCondition()
    {
        // Arrange
        // Act
        var result = await _decorator.Get(x => x.Created == new DateTime(2010, 04, 10, 12, 1, 1), x => x.Winner, new TenantId(7), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.Single().Should().NotBeNull();
    }

    [Fact]
    public async Task GetWithSort_IncludeTenantCondition()
    {
        // Arrange
        // Act
        var result = await _decorator.Get(x => x.Created == new DateTime(2010, 04, 10, 12, 1, 1), new TenantId(7), x => x.id, 1, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.Single().Winner.Should().NotBeNull();
    }

    private sealed class UsersRepositoryForTests : IVotingSessionQueryRepository
    {
        public readonly VotingResult[] MockVotingResults =
        [
            new() { TenantId = 21 },
            new() { TenantId = 3, Created = new DateTime(2010, 04, 10, 12, 1, 1)},
            new() { TenantId = 7, Created = new DateTime(2010, 04, 10, 12, 1, 1), Winner = new EmbeddedMovie()},
            new() { TenantId = 7, Created = new DateTime(2010, 04, 10, 11, 1, 1)},
        ];

        public Task<IReadonlyVotingResult?> Get(Expression<Func<IReadonlyVotingResult, bool>> predicate, TenantId tenant, CancellationToken cancellationToken)
        {
            var func = predicate.Compile();
            var result = MockVotingResults.Single(func);
            return Task.FromResult<IReadonlyVotingResult?>(result);
        }

        public Task<IEnumerable<IReadonlyVotingResult>> Get(Expression<Func<IReadonlyVotingResult, bool>> predicate, TenantId tenant, Expression<Func<IReadonlyVotingResult, object>> sortBy, int take, CancellationToken cancellationToken)
        {
            var func = predicate.Compile();
            var result = MockVotingResults.Where(func);
            return Task.FromResult(result);
        }

        public Task<IEnumerable<T>> Get<T>(Expression<Func<IReadonlyVotingResult, bool>> predicate, Expression<Func<IReadonlyVotingResult, T>> selector, TenantId tenant, CancellationToken cancellationToken) where T : class
        {
            var func = predicate.Compile();
            var selc = selector.Compile();
            var result = MockVotingResults.Where(func).Select(selc);
            return Task.FromResult(result);
        }
    }
}