using AutoFixture;
using FluentAssertions;
using Filmowanie.Account.Constants;
using Filmowanie.Account.DTOs.Incoming;
using Filmowanie.Account.Validators;

namespace Filmowanie.Tests.Filmowanie_Account;

public sealed class UserDTOValidatorTests
{
    private readonly UserDTOValidator _sut;
    private readonly IFixture _fixture;

    public UserDTOValidatorTests()
    {
        _sut = new UserDTOValidator();
        _fixture = new Fixture();
    }

    [Fact]
    public void ValidateUserId_WhenUserIdIsNull_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new UserDTO(null!, "Mr Bean");

        // Act
        var result = _sut.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Value cannot be null!");
    }

    [Fact]
    public void ValidateUserId_WhenUserIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new UserDTO(string.Empty, "Mr Bean");

        // Act
        var result = _sut.Validate(dto);

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
        var dto = new UserDTO(invalidUserId, "Mr Bean");

        // Act
        var result = _sut.Validate(dto);

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
        var dto = new UserDTO(shortUserId, "Mr Bean");

        // Act
        var result = _sut.Validate(dto);

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
        var dto = new UserDTO(invalidUserId, "Mr Bean");

        // Act
        var result = _sut.Validate(dto);

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
        var dto = new UserDTO(userId, "Mr Bean");

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
        var canHandle = _sut.CanHandle<UserDTO>(KeyedServices.Username, out var validator);

        // Assert
        canHandle.Should().BeTrue();
        validator.Should().NotBeNull();
        validator.Should().BeOfType<UserDTOValidator>();
    }

    [Fact]
    public void CanHandle_WhenTypeDoesNotMatch_ShouldReturnFalseAndNullValidator()
    {
        // Act
        var canHandle = _sut.CanHandle<string>(KeyedServices.Username, out var validator);

        // Assert
        canHandle.Should().BeFalse();
        validator.Should().BeNull();
    }

    [Fact]
    public void CanHandle_WhenKeyDoesNotMatch_ShouldReturnFalseAndNullValidator()
    {
        // Act
        var canHandle = _sut.CanHandle<UserDTO>("wrong-key", out var validator);

        // Assert
        canHandle.Should().BeFalse();
        validator.Should().BeNull();
    }
}
