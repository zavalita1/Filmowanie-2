using AutoFixture;
using Filmowanie.Account.Constants;
using Filmowanie.Account.DTOs.Incoming;
using Filmowanie.Account.Validators;
using FluentAssertions;

namespace Filmowanie.Tests.Filmowanie_Account;

public sealed class BasicAuthSignupValidatorTests : BasicAuthValidatorTests
{
    private readonly BasicAuthSignupValidator _validator = new();

    [Fact]
    public void Validate_ShouldReturnError_WhenPasswordIsTooShort()
    {
        // Arrange
        var dto = new BasicAuthLoginDTO("email@to.com", "short");

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Password must have 7 characters or more");
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenPasswordIsLongEnough()
    {
        // Arrange
        var dto = new BasicAuthLoginDTO("email@to.com", "longenough");

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CanHandle_ShouldReturnTrue_ForValidTypeAndKey()
    {
        // Act
        var canHandle = _validator.CanHandle<BasicAuthLoginDTO>(KeyedServices.SignUpBasicAuth, out var typedValidator);

        // Assert
        canHandle.Should().BeTrue();
        typedValidator.Should().NotBeNull();
    }

    [Fact]
    public void CanHandle_ShouldReturnFalse_ForInvalidType()
    {
        // Act
        var canHandle = _validator.CanHandle<object>(KeyedServices.SignUpBasicAuth, out var typedValidator);

        // Assert
        canHandle.Should().BeFalse();
        typedValidator.Should().BeNull();
    }

    [Fact]
    public void CanHandle_ShouldReturnFalse_ForInvalidKey()
    {
        // Act
        var canHandle = _validator.CanHandle<BasicAuthLoginDTO>("InvalidKey", out var typedValidator);

        // Assert
        canHandle.Should().BeFalse();
        typedValidator.Should().BeNull();
    }
}