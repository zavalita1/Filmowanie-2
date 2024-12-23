using Filmowanie.Abstractions.Providers;
using FluentAssertions;

namespace Filmowanie.Tests.Filmowanie_Abstractions;

public class UnitTest1
{
    [Fact]
    public void Now_ShouldReturnCurrentUtcDateTime()
    {
        // Arrange
        var dateTimeProvider = new DateTimeProvider();
        var lowerBound = DateTime.UtcNow;

        // Act
        var result = dateTimeProvider.Now;
        var upperBound = DateTime.UtcNow;

        // Assert
        result.Should().BeOnOrBefore(upperBound);
        result.Should().BeOnOrAfter(lowerBound);
    }
}