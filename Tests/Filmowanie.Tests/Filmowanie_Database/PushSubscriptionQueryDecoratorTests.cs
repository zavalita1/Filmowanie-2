using Filmowanie.Database.Decorators;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using NSubstitute;
using System.Linq.Expressions;
using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Database.Entities;
using FluentAssertions;

namespace Filmowanie.Tests.Filmowanie_Database;

public sealed class PushSubscriptionQueryDecoratorTests
{
    private readonly PushSubscriptionQueryDecorator _sut;
    private readonly IPushSubscriptionQueryRepository _pushNotificationRepo;

    public PushSubscriptionQueryDecoratorTests()
    {
        _pushNotificationRepo = Substitute.For<IPushSubscriptionQueryRepository>();
        _sut = new PushSubscriptionQueryDecorator(_pushNotificationRepo);
    }

    [Fact]
    public async Task GetAllAsync_ModifiesPredicateAccordingly()
    {
        // Arrange
        var stubbedDbEntities = new[]
        {
            new ReadOnlyPushSubscriptionEntity { id = "loo", Auth = "double loo", Endpoint = "3333" , TenantId = 213 },
            new ReadOnlyPushSubscriptionEntity { id = "loo", Auth= "double loo",  Endpoint = "1111" , TenantId = 2137 },
            new ReadOnlyPushSubscriptionEntity { id = "loo2", Auth = "double loo",  Endpoint = "5555" , TenantId = 2137 },
            new ReadOnlyPushSubscriptionEntity { id = "loo2",Auth = "double loo", Endpoint = "4444" , TenantId = 2137 },
            new ReadOnlyPushSubscriptionEntity { id = "loo", Auth = "double loo", Endpoint = "2222" , TenantId = 2137 },
            new ReadOnlyPushSubscriptionEntity { id = "loo", Auth = "double loo", Endpoint = "6666" , TenantId = 21372 },
        };
        _pushNotificationRepo.GetAsync(default!, default, CancellationToken.None)
            .ReturnsForAnyArgs(ci => stubbedDbEntities.Where(x => ci.ArgAt<Expression<Func<IReadOnlyPushSubscriptionEntity, bool>>>(0).Compile().Invoke(x)).ToArray<IReadOnlyPushSubscriptionEntity>());

        // Act
        var result = await _sut.GetAllAsync(new TenantId(2137), CancellationToken.None);

        // Assert
        result.Should().HaveCount(4);
        result[0].Endpoint.Should().Be("1111");
        result[1].Endpoint.Should().Be("5555");
        result[2].Endpoint.Should().Be("4444");
        result[3].Endpoint.Should().Be("2222");
    }


    [Fact]
    public async Task GetAsync_ModifiesPredicateAccordingly()
    {
        // Arrange
        var stubbedDbEntities = new[]
        {
            new ReadOnlyPushSubscriptionEntity { id = "loo", Auth = "double loo", Endpoint = "3333" , TenantId = 213 },
            new ReadOnlyPushSubscriptionEntity { id = "loo", Auth= "double loo",  Endpoint = "1111" , TenantId = 2137 },
            new ReadOnlyPushSubscriptionEntity { id = "loo2", Auth = "double loo",  Endpoint = "5555" , TenantId = 2137 },
            new ReadOnlyPushSubscriptionEntity { id = "loo2",Auth = "double loo", Endpoint = "4444" , TenantId = 2137 },
            new ReadOnlyPushSubscriptionEntity { id = "loo", Auth = "double loo", Endpoint = "2222" , TenantId = 2137 },
        };
        _pushNotificationRepo.GetAsync(default!, default, CancellationToken.None)
            .ReturnsForAnyArgs(ci => stubbedDbEntities.Where(x => ci.ArgAt<Expression<Func<IReadOnlyPushSubscriptionEntity, bool>>>(0).Compile().Invoke(x)).ToArray<IReadOnlyPushSubscriptionEntity>());

        // Act
        var result = await _sut.GetAsync(x => x.id == "loo" && x.Auth == "double loo", new TenantId(2137), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.First().Endpoint.Should().Be("1111");
        result.Last().Endpoint.Should().Be("2222");
    }
}