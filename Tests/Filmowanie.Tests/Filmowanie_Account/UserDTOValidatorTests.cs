using Filmowanie.Account.Constants;
using Filmowanie.Account.DTOs.Incoming;
using Filmowanie.Account.Validators;
using FluentAssertions;
using FluentValidation;
using NSubstitute;

namespace Filmowanie.Tests.Filmowanie_Account;

public sealed class UserDTOValidatorTests
{
    private readonly UserDTOValidator _validator;
    private readonly IValidator<UserDTO> _userIdValidator;

    public UserDTOValidatorTests()
    {
        _userIdValidator = Substitute.For<IValidator<UserDTO>>();
        _validator = new UserDTOValidator();
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenValueIsEmpty()
    {
        // Arrange
        var value = "";
        var dto = new UserDTO(value, "whatever");

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Value cannot be empty!");
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenValueIsTooShort()
    {
        // Arrange
        var value = "short";
        var dto = new UserDTO(value, "whatever");

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Length must be between 6 and 30 characters");
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenValueIsTooLong()
    {
        // Arrange
        var value = new string('a', 31);
        var dto = new UserDTO(value, "whatever");

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Length must be between 6 and 30 characters");
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenValueContainsIllegalCharacters()
    {
        // Arrange
        var value = "invalid@value";
        var dto = new UserDTO(value, "whatever");

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Can't contain illegal characters");
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenValueIsValid()
    {
        // Arrange
        var value = "valid_value";
        var dto = new UserDTO(value, "whatever");

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CanHandle_ShouldReturnTrue_ForValidTypeAndKey()
    {
        // Act
        var canHandle = _validator.CanHandle<UserDTO>(KeyedServices.Username, out var typedValidator);

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
        var canHandle = _validator.CanHandle<UserDTO>("InvalidKey", out var typedValidator);

        // Assert
        canHandle.Should().BeFalse();
        typedValidator.Should().BeNull();
    }
}