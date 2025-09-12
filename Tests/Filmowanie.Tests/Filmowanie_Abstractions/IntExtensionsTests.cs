using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using FluentAssertions;

namespace Filmowanie.Tests.Filmowanie_Abstractions;

public sealed class IntExtensionsTests
{
    [Theory]
    [InlineData(1990, Decade._1990s)]
    [InlineData(1991, Decade._1990s)]
    [InlineData(1992, Decade._1990s)]
    [InlineData(1999, Decade._1990s)]
    [InlineData(2000, Decade._2000s)]
    [InlineData(1940, Decade._1940s)]
    [InlineData(1950, Decade._1950s)]
    [InlineData(1960, Decade._1960s)]
    [InlineData(1970, Decade._1970s)]
    [InlineData(1980, Decade._1980s)]
    [InlineData(2010, Decade._2010s)]
    [InlineData(2020, Decade._2020s)]
    public void ToDecade_ShouldProperlyMap(int year, Decade expectedResult)
    {
        // Arrange
        // Act
        var result = year.ToDecade();

        // Assert
        result.Should().Be(expectedResult);
    }


    [Theory]
    [InlineData(1939)]
    [InlineData(0)]
    [InlineData(-2137)]
    public void ToDecade_ShouldThrowForInvalidArgument(int year)
    {
        // Arrange
        var action = () => year.ToDecade();

        // Act
        // Assert
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(1, "1 min.")]
    [InlineData(59, "59 min.")]
    [InlineData(60, "1 godz.")]
    [InlineData(61, "1 godz. 1 min.")]
    [InlineData(207, "3 godz. 27 min.")]
    public void GetDurationString_ShouldThrowForInvalidArgument(int durationInMinutes, string expectedDurationString)
    {
        // Arrange
        // Act
        var result = durationInMinutes.GetDurationString();

        // Assert
        result.Should().Be(expectedDurationString);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-2137)]
    [InlineData(int.MinValue)]
    public void GetDurationString_ShouldThrowForNonpositiveArgument(int year)
    {
        // Arrange
        var action = () => year.GetDurationString();

        // Act
        // Assert
        action.Should().Throw<ArgumentException>();
    }
}