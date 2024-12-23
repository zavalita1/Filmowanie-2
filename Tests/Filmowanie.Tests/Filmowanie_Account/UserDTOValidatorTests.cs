using Filmowanie.Account.Constants;
using Filmowanie.Account.DTOs.Incoming;
using Filmowanie.Account.Validators;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
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
    public void Validate_ShouldInvokeUserIdValidator()
    {
        // Arrange
        var dto = new UserDTO("valid-id", "whatever");
        _userIdValidator.Validate(Arg.Any<ValidationContext<UserDTO>>()).Returns(new ValidationResult());

        // Act
        var result = _validator.Validate(dto);

        // Assert
        _userIdValidator.Received(1).Validate(Arg.Any<ValidationContext<UserDTO>>());
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