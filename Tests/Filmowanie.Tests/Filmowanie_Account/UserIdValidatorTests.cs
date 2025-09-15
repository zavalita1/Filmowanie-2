using AutoFixture;
using FluentAssertions;
using Filmowanie.Account.Constants;
using Filmowanie.Account.Validators;

namespace Filmowanie.Tests.Filmowanie_Account;

public sealed class UserIdValidatorTests
{
    private readonly UserIdValidator _sut;
    private readonly IFixture _fixture;

    public UserIdValidatorTests()
    {
        _sut = new UserIdValidator();
        _fixture = new Fixture();
    }

    [Fact]
    public void ValidateUserId_WhenUserIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var userId = string.Empty;

        // Act
        var result = _sut.Validate(userId);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Value cannot be empty!");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("usr-12345")]
    [InlineData("users-12345")]
    public void ValidateUserId_WhenUserIdDoesNotStartWithPrefix_ShouldHaveValidationError(string invalidUserId)
    {
        // Arrange
        // Act
        var result = _sut.Validate(invalidUserId);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Must start with proper prefix!");
    }

    [Theory]
    [InlineData("user-")]
    [InlineData("user")]
    public void ValidateUserId_WhenUserIdIsTooShort_ShouldHaveValidationError(string shortUserId)
    {
        // Arrange
        // Act
        var result = _sut.Validate(shortUserId);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Length must be between 6 and 9999 characters");
    }

    [Theory]
    [InlineData("user-123")]
    [InlineData("user-abc")]
    [InlineData("user-not-guid")]
    public void ValidateUserId_WhenUserIdSuffixIsNotGuid_ShouldHaveValidationError(string invalidUserId)
    {
        // Arrange
        // Act
        var result = _sut.Validate(invalidUserId);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Must be guid-parsable!");
    }

    [Fact]
    public void ValidateUserId_WhenUserIdIsValid_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var guid = Guid.NewGuid().ToString();
        var userId = $"user-{guid}";

        // Act
        var result = _sut.Validate(userId);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void CanHandle_WhenTypeAndKeyMatch_ShouldReturnTrueAndValidator()
    {
        // Act
        var canHandle = _sut.CanHandle<string>(KeyedServices.Username, out var validator);

        // Assert
        canHandle.Should().BeTrue();
        validator.Should().NotBeNull();
        validator.Should().BeOfType<UserIdValidator>();
    }

    [Fact]
    public void CanHandle_WhenTypeDoesNotMatch_ShouldReturnFalseAndNullValidator()
    {
        // Act
        var canHandle = _sut.CanHandle<int>(KeyedServices.Username, out var validator);

        // Assert
        canHandle.Should().BeFalse();
        validator.Should().BeNull();
    }

    [Fact]
    public void CanHandle_WhenKeyDoesNotMatch_ShouldReturnFalseAndNullValidator()
    {
        // Act
        var canHandle = _sut.CanHandle<string>("wrong-key", out var validator);

        // Assert
        canHandle.Should().BeFalse();
        validator.Should().BeNull();
    }
}
