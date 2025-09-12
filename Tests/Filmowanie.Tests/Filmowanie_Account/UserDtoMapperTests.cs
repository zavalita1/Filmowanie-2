using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.Mappers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Filmowanie.Tests.Filmowanie_Account;

public sealed class UserDtoMapperTests
{
    private readonly UserDtoMapper _sut;

    public UserDtoMapperTests()
    {
        var logger = Substitute.For<ILogger<UserDtoMapper>>();
        _sut = new UserDtoMapper(logger);
    }

    [Fact]
    public void Map_WhenInputIsValid_ReturnsUserDto()
    {
        // Arrange
        var name = "Mr Bean";
        var isAdmin = true;
        var hasBasicAuth = true;
        var user = new DomainUser(
            "user-2137",
            "ext-42",
            isAdmin,
            false,
            new TenantId(21),
            DateTime.UtcNow)
        {
            Name = name,
            HasBasicAuthSetup = hasBasicAuth
        };

        var input = new Maybe<DomainUser>(user, null);

        // Act
        var result = _sut.Map(input);

        // Assert
        result.Result.Should().NotBeNull();
        result.Error.Should().BeNull();
        
        var userDto = result.Result;
        userDto.Should().NotBeNull();
        userDto!.Username.Should().Be(name);
        userDto.IsAdmin.Should().Be(isAdmin);
        userDto.HasRegisteredBasicAuth.Should().Be(hasBasicAuth);
    }

    [Fact]
    public void Map_WhenInputHasError_ReturnsError()
    {
        // Arrange
        var error = new Error<DomainUser>("", ErrorType.InvalidState);
        var input = new Maybe<DomainUser>(default, error);

        // Act
        var result = _sut.Map(input);

        // Assert
        result.Result.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Value.Type.Should().Be(ErrorType.InvalidState);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Map_WithDifferentNameValues_MapsCorrectly(string? name)
    {
        // Arrange
        var user = new DomainUser(
            "user-2137",
            "ext-42",
            false,
            false,
            new TenantId(21),
            DateTime.UtcNow)
        {
            Name = name
        };

        var input = new Maybe<DomainUser>(user, null);

        // Act
        var result = _sut.Map(input);

        // Assert
        result.Result.Should().NotBeNull();
        result.Result!.Username.Should().Be(name);
    }
}
