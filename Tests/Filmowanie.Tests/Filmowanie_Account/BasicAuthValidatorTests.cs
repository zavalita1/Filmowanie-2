using Filmowanie.Account.DTOs.Incoming;
using Filmowanie.Account.Validators;
using FluentAssertions;

namespace Filmowanie.Tests.Filmowanie_Account;

public class BasicAuthValidatorTests
{
    private readonly BasicAuthValidator _validator;

    public BasicAuthValidatorTests()
    {
        _validator = new BasicAuthValidator();
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenEmailIsNull()
    {
        // Arrange
        var dto = new BasicAuthLoginDTO(null, "password");

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Email cannot be null!");
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenEmailIsEmpty()
    {
        // Arrange
        var dto = new BasicAuthLoginDTO(string.Empty, "password");

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Email cannot be empty!");
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenEmailIsInvalid()
    {
        // Arrange
        var dto = new BasicAuthLoginDTO { Email = "invalid-email", Password = "password" };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Email must be a valid mail address!");
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenPasswordIsNull()
    {
        // Arrange
        var dto = new BasicAuthLoginDTO { Email = "test@example.com", Password = null };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Email cannot be null!");
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenPasswordIsEmpty()
    {
        // Arrange
        var dto = new BasicAuthLoginDTO { Email = "test@example.com", Password = "" };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Email cannot be empty!");
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenEmailAndPasswordAreValid()
    {
        // Arrange
        var dto = new BasicAuthLoginDTO { Email = "test@example.com", Password = "password" };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CanHandle_ShouldReturnTrue_ForValidTypeAndKey()
    {
        // Act
        var canHandle = _validator.CanHandle<BasicAuthLoginDTO>(KeyedServices.LoginViaBasicAuthKey, out var typedValidator);

        // Assert
        canHandle.Should().BeTrue();
        typedValidator.Should().NotBeNull();
    }

    [Fact]
    public void CanHandle_ShouldReturnFalse_ForInvalidType()
    {
        // Act
        var canHandle = _validator.CanHandle<object>(KeyedServices.LoginViaBasicAuthKey, out var typedValidator);

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