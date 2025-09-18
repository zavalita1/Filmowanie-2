using AutoFixture;
using FluentAssertions;
using Filmowanie.Account.Constants;
using Filmowanie.Account.DTOs.Incoming;
using Filmowanie.Account.Validators;

namespace Filmowanie.Tests.Filmowanie_Account;

public sealed class BasicAuthValidatorTests
{
    private readonly BasicAuthValidator _sut;
    private readonly IFixture _fixture;

    public BasicAuthValidatorTests()
    {
        _sut = new BasicAuthValidator();
        _fixture = new Fixture();
    }

    [Fact]
    public void ValidateEmail_WhenEmailIsNull_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new BasicAuthLoginDTO(null!, "password123");

        // Act
        var result = _sut.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(BasicAuthLoginDTO.Email) 
            && x.ErrorMessage == $"{nameof(BasicAuthLoginDTO.Email)} cannot be null!");
    }

    [Fact]
    public void ValidateEmail_WhenEmailIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new BasicAuthLoginDTO(string.Empty, "password123");

        // Act
        var result = _sut.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(BasicAuthLoginDTO.Email) 
            && x.ErrorMessage == $"{nameof(BasicAuthLoginDTO.Email)} cannot be empty!");
    }

    [Theory]
    [InlineData("notAnEmail")]
    [InlineData("@nocontent.com")]
    [InlineData("noat.com")]
    public void ValidateEmail_WhenEmailIsInvalid_ShouldHaveValidationError(string invalidEmail)
    {
        // Arrange
        var dto = new BasicAuthLoginDTO(invalidEmail, "password123");

        // Act
        var result = _sut.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(BasicAuthLoginDTO.Email) 
            && x.ErrorMessage == $"{nameof(BasicAuthLoginDTO.Email)} must be a valid mail address!");
    }

    [Fact]
    public void ValidatePassword_WhenPasswordIsNull_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new BasicAuthLoginDTO("mr.bean@atkinson.com", null!);

        // Act
        var result = _sut.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(BasicAuthLoginDTO.Password) 
            && x.ErrorMessage == $"{nameof(BasicAuthLoginDTO.Email)} cannot be null!");
    }

    [Fact]
    public void ValidatePassword_WhenPasswordIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new BasicAuthLoginDTO("mr.bean@atkinson.com", string.Empty);

        // Act
        var result = _sut.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(BasicAuthLoginDTO.Password) 
            && x.ErrorMessage == $"{nameof(BasicAuthLoginDTO.Email)} cannot be empty!");
    }

    [Fact]
    public void ValidateDTO_WhenDTOIsValid_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var dto = new BasicAuthLoginDTO("mr.bean@atkinson.com", "password123");

        // Act
        var result = _sut.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void CanHandle_WhenTypeAndKeyMatch_ShouldReturnTrueAndValidator()
    {
        // Act
        var canHandle = _sut.CanHandle<BasicAuthLoginDTO>(KeyedServices.LoginViaBasicAuthKey, out var validator);

        // Assert
        canHandle.Should().BeTrue();
        validator.Should().NotBeNull();
        validator.Should().BeOfType<BasicAuthValidator>();
    }

    [Fact]
    public void CanHandle_WhenTypeDoesNotMatch_ShouldReturnFalseAndNullValidator()
    {
        // Act
        var canHandle = _sut.CanHandle<string>(KeyedServices.LoginViaBasicAuthKey, out var validator);

        // Assert
        canHandle.Should().BeFalse();
        validator.Should().BeNull();
    }

    [Fact]
    public void CanHandle_WhenKeyDoesNotMatch_ShouldReturnFalseAndNullValidator()
    {
        // Act
        var canHandle = _sut.CanHandle<BasicAuthLoginDTO>("wrong-key", out var validator);

        // Assert
        canHandle.Should().BeFalse();
        validator.Should().BeNull();
    }
}
