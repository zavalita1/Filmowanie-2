using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.DTOs.Incoming;
using Filmowanie.Account.Mappers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Filmowanie.Tests.Filmowanie_Account;

public sealed class DomainUserMapperTests
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IGuidProvider _guidProvider;
    private readonly DomainUserMapper _sut;

    public DomainUserMapperTests()
    {
        var logger = Substitute.For<ILogger<DomainUserMapper>>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _guidProvider = Substitute.For<IGuidProvider>();
        _sut = new DomainUserMapper(logger, _dateTimeProvider, _guidProvider);
    }

    [Fact]
    public void Map_WhenInputIsValid_ReturnsDomainUser()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var guid = Guid.NewGuid();
        var expectedUserId = $"user-{guid}";
        var tenantId = new TenantId(2137);
        var userDto = new UserDTO("external-id-42", "whatever");
        var currentUser = new DomainUser("current-user-id", "current-external-id", false, true, tenantId, DateTime.UtcNow);
        
        _dateTimeProvider.Now.Returns(now);
        _guidProvider.NewGuid().Returns(guid);

        var input = new Maybe<(UserDTO, DomainUser)>((userDto, currentUser), null);

        // Act
        var result = _sut.Map(input);

        // Assert
        result.Result.Should().NotBeNull();
        result.Error.Should().BeNull();
        
        var domainUser = result.Result;
        domainUser.Should().NotBeNull();
        domainUser!.Id.Should().Be(expectedUserId);
        domainUser.IsAdmin.Should().BeFalse();
        domainUser.Tenant.Should().Be(tenantId);
        domainUser.Created.Should().Be(now);
    }

    [Fact]
    public void Map_WhenInputHasError_ReturnsError()
    {
        // Arrange
        var error = new Error<(UserDTO, DomainUser)>("", ErrorType.InvalidState);
        var input = new Maybe<(UserDTO, DomainUser)>(default, error);

        // Act
        var result = _sut.Map(input);

        // Assert
        result.Result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Value.Type.Should().Be(ErrorType.InvalidState);
    }
}
