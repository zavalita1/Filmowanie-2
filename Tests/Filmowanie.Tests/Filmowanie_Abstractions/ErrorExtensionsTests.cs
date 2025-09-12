using AutoFixture;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Maybe;
using FluentAssertions;

namespace Filmowanie.Tests.Filmowanie_Abstractions;


public sealed class ErrorExtensionsTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void ChangeResultType_ShouldKeepErrorTypeAndMessage()
    {
        // Arrange
        var errorMessage = _fixture.Create<string>();
        var errorType = _fixture.Create<ErrorType>();
        var error1 = new Error<string>(errorMessage, errorType);

        // Act
        var result = error1.ChangeResultType<string, object>();

        // Assert
        result.Type.Should().Be(errorType);
        result.ToString().Should().Be(errorMessage);
    }

    [Fact]
    public void ChangeResultType_ShouldKeepErrorTypeAndMessages()
    {
        // Arrange
        var errorMessage1 = _fixture.Create<string>();
        var errorMessage2 = _fixture.Create<string>();
        var errorType = _fixture.Create<ErrorType>();
        var error1 = new Error<string>([errorMessage1, errorMessage2], errorType);

        // Act
        var result = error1.ChangeResultType<string, object>();

        // Assert
        result.Type.Should().Be(errorType);
        result.ToString().Should().Be($"{errorMessage1},{errorMessage2}");
    }
}