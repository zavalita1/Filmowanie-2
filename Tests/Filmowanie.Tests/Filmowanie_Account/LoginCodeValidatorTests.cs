using Filmowanie.Account.DTOs.Incoming;
using Filmowanie.Account.Validators;
using FluentAssertions;

namespace Filmowanie.Tests.Filmowanie_Account;

public class LoginCodeValidatorTests
{
    private readonly LoginCodeValidator _validator;

    public LoginCodeValidatorTests()
    {
        _validator = new LoginCodeValidator();
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenCodeIsNull()
    {
        // Arrange
        var dto = new LoginDto(null);

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Code cannot be null!");
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenCodeIsEmpty()
    {
        // Arrange
        var dto = new LoginDto("");

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Code cannot be empty!");
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenCodeIsInvalidGuid()
    {
        // Arrange
        var dto = new LoginDto ("invalid-guid");

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Code must be a valid guid!");
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenCodeIsValidGuid()
    {
        // Arrange
        var dto = new LoginDto(Guid.NewGuid().ToString());

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CanHandle_ShouldReturnTrue_ForValidType()
    {
        // Act
        var canHandle = _validator.CanHandle<LoginDto>("anyKey", out var typedValidator);

        // Assert
        canHandle.Should().BeTrue();
        typedValidator.Should().NotBeNull();
    }

    [Fact]
    public void CanHandle_ShouldReturnFalse_ForInvalidType()
    {
        // Act
        var canHandle = _validator.CanHandle<object>("anyKey", out var typedValidator);

        // Assert
        canHandle.Should().BeFalse();
        typedValidator.Should().BeNull();
    }
}