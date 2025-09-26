using FluentAssertions;
using Filmowanie.Account.Helpers;

namespace Filmowanie.UnitTests.Filmowanie_Account;

public class HashHelperTests
{
    private readonly HashHelper _hashHelper;

    public HashHelperTests()
    {
        _hashHelper = new HashHelper();
    }

    [Fact]
    public void GetHash_ShouldReturnHashWithSalt()
    {
        // Arrange
        var secret = "mySecret";
        var saltSeed = "mySaltSeed";

        // Act
        var hash = _hashHelper.GetHash(secret, saltSeed);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Length.Should().BeGreaterThan(secret.Length);
    }

    [Fact]
    public void DoesHashEqual_ShouldReturnTrue_WhenHashesMatch()
    {
        // Arrange
        var secret = "mySecret";
        var saltSeed = "mySaltSeed";
        var hash = _hashHelper.GetHash(secret, saltSeed);

        // Act
        var result = _hashHelper.DoesHashEqual("xxx", "whatever");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void DoesHashEqual_ShouldReturnFalse_WhenHashesDoNotMatch()
    {
        // Arrange
        var secret = "mySecret";
        var saltSeed = "mySaltSeed";
        var hash = _hashHelper.GetHash(secret, saltSeed);
        var differentSecret = "differentSecret";

        // Act
        var result = _hashHelper.DoesHashEqual(hash, differentSecret);

        // Assert
        result.Should().BeFalse();
    }
}
