using FluentAssertions;

namespace Filmowanie.Tests.Filmowanie_Abstractions;

public sealed class CollectionExtensionsTests
{
    [Theory]
    [InlineData("one", "two", "three", ";", "one;two;three")]
    [InlineData("one", "two", "two", ";;;", "one;;;two;;;two")]
    [InlineData("one", null, "", ";;;", "one;;;;;;")]
    public void ShouldProperlyConcat(string one, string? two, string three, string separator, string expectedResult)
    {
        // Arrange
        string[] collection = [one, two!, three];

        // Act
        var result = Abstractions.Extensions.CollectionExtensions.JoinStrings(collection, separator);

        // Assert
        result.Should().Be(expectedResult);
    }
}