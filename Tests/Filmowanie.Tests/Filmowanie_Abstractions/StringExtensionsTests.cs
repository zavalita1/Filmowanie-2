using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using FluentAssertions;

namespace Filmowanie.Tests.Filmowanie_Abstractions;

public sealed class StringExtensionsTests
{
    [Theory]
    [InlineData("1990s", Decade._1990s)]
    [InlineData("1980s", Decade._1980s)]
    public void ToDecade_ShouldProperlyMap(string year, Decade expectedResult)
    {
        // Arrange
        // Act
        var result = year.ToDecade();

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void ToDecade_ShouldThrowWhenStringEndsWithNonS()
    {
        // Arrange
        var argument = "whatever";
        var act = () => argument.ToDecade();

        // Act
        // Assert
        act.Should().Throw<ArgumentException>();
    }
}