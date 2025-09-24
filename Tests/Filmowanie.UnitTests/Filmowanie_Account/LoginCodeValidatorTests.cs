using AutoFixture;
using FluentAssertions;
using Filmowanie.Account.DTOs.Incoming;
using Filmowanie.Account.Validators;

namespace Filmowanie.UnitTests.Filmowanie_Account;

public sealed class LoginCodeValidatorTests
{
    private readonly LoginCodeValidator _sut;
    private readonly IFixture _fixture;

    public LoginCodeValidatorTests()
    {
        _sut = new LoginCodeValidator();
        _fixture = new Fixture();
    }

    [Fact]
    public void ValidateCode_WhenCodeIsNull_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new LoginDto(null!);

        // Act
        var result = _sut.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(LoginDto.Code) 
            && x.ErrorMessage == $"{nameof(LoginDto.Code)} cannot be null!");
    }

    [Fact]
    public void ValidateCode_WhenCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new LoginDto(string.Empty);

        // Act
        var result = _sut.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(LoginDto.Code) 
            && x.ErrorMessage == $"{nameof(LoginDto.Code)} cannot be empty!");
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("123")]
    [InlineData("xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx")]
    public void ValidateCode_WhenCodeIsNotValidGuid_ShouldHaveValidationError(string invalidCode)
    {
        // Arrange
        var dto = new LoginDto(invalidCode);

        // Act
        var result = _sut.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(LoginDto.Code) 
            && x.ErrorMessage == $"{nameof(LoginDto.Code)} must be a valid guid!");
    }

    [Fact]
    public void ValidateDto_WhenCodeIsValidGuid_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var dto = new LoginDto(Guid.NewGuid().ToString());

        // Act
        var result = _sut.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void CanHandle_WhenTypeMatches_ShouldReturnTrueAndValidator()
    {
        // Act
        var canHandle = _sut.CanHandle<LoginDto>("any-key", out var validator);

        // Assert
        canHandle.Should().BeTrue();
        validator.Should().NotBeNull();
        validator.Should().BeOfType<LoginCodeValidator>();
    }

    [Fact]
    public void CanHandle_WhenTypeDoesNotMatch_ShouldReturnFalseAndNullValidator()
    {
        // Act
        var canHandle = _sut.CanHandle<string>("any-key", out var validator);

        // Assert
        canHandle.Should().BeFalse();
        validator.Should().BeNull();
    }
}
