using Filmowanie.Account.Constants;
using Filmowanie.Account.Validators;
using FluentAssertions;

namespace Filmowanie.Tests.Filmowanie_Account;

public sealed class UserIdValidatorTests
{
    private readonly UserIdValidator _validator = new();

    [Fact]
    public void Validate_ShouldReturnError_WhenValueIsNull()
    {
        // Arrange
        string value = null;

        // Act
        var result = _validator.Validate(value);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Value cannot be null!");
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenValueIsEmpty()
    {
        // Arrange
        var value = "";

        // Act
        var result = _validator.Validate(value);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Value cannot be empty!");
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenValueIsTooShort()
    {
        // Arrange
        var value = "short";

        // Act
        var result = _validator.Validate(value);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Length must be between 6 and 30 characters");
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenValueIsTooLong()
    {
        // Arrange
        var value = new string('a', 31);

        // Act
        var result = _validator.Validate(value);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Length must be between 6 and 30 characters");
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenValueContainsIllegalCharacters()
    {
        // Arrange
        var value = "invalid@value";

        // Act
        var result = _validator.Validate(value);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Can't contain illegal characters");
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenValueIsValid()
    {
        // Arrange
        var value = "valid_value";

        // Act
        var result = _validator.Validate(value);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CanHandle_ShouldReturnTrue_ForValidTypeAndKey()
    {
        // Act
        var canHandle = _validator.CanHandle<string>(KeyedServices.Username, out var typedValidator);

        // Assert
        canHandle.Should().BeTrue();
        typedValidator.Should().NotBeNull();
    }

    [Fact]
    public void CanHandle_ShouldReturnFalse_ForInvalidType()
    {
        // Act
        var canHandle = _validator.CanHandle<object>(KeyedServices.Username, out var typedValidator);

        // Assert
        canHandle.Should().BeFalse();
        typedValidator.Should().BeNull();
    }

    [Fact]
    public void CanHandle_ShouldReturnFalse_ForInvalidKey()
    {
        // Act
        var canHandle = _validator.CanHandle<string>("InvalidKey", out var typedValidator);

        // Assert
        canHandle.Should().BeFalse();
        typedValidator.Should().BeNull();
    }
}