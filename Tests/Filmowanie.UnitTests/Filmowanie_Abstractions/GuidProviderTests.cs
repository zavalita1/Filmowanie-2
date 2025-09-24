using Filmowanie.Abstractions.Wrappers;
using FluentAssertions;

namespace Filmowanie.UnitTests.Filmowanie_Abstractions;

public class GuidProviderTests
{
    [Fact]
    public void NewGuid_ShouldReturnUniqueGuid()
    {
        // Arrange
        var guidProvider = new GuidProvider();

        // Act
        var guid1 = guidProvider.NewGuid();
        var guid2 = guidProvider.NewGuid();

        // Assert
        guid1.Should().NotBeEmpty();
        guid2.Should().NotBeEmpty();
        guid1.Should().NotBe(guid2);
    }
}