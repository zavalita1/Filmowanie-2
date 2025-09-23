using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Maybe;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Filmowanie.Tests.Filmowanie_Abstractions;

public sealed class MaybeExtensionsTests
{
    private readonly ILogger _logger = Substitute.For<ILogger>();

    [Fact]
    public void RequireResult_WhenMaybeHasResult_ReturnsResult()
    {
        // Arrange
        var value = 42;
        var maybe = value.AsMaybe();

        // Act
        var result = maybe.RequireResult();

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public void Cancelled_ReturnsErrorMaybe()
    {
        // Act
        var maybe = MaybeExtensions.Cancelled<int>();

        // Assert
        maybe.Error.Should().NotBeNull();
        maybe.Error!.Value.Type.Should().Be(ErrorType.Canceled);
        maybe.Error!.Value.ErrorMessages.Should().Contain("Operation canceled.");
    }

    [Fact]
    public void RequireResult_WhenMaybeHasError_ThrowsException()
    {
        // Arrange
        var maybe = MaybeExtensions.Cancelled<int>();

        // Act & Assert
        var action = () => maybe.RequireResult();
        action.Should().Throw<Exception>()
            .WithMessage("Required unwrap failed, as there was an error result!");
    }

    [Fact]
    public void AsMaybe_CreatesSuccessfulMaybe()
    {
        // Arrange
        var value = 42;

        // Act
        var maybe = value.AsMaybe();

        // Assert
        maybe.Result.Should().Be(value);
        maybe.Error.Should().BeNull();
    }

    [Fact]
    public void Map_WhenSuccessful_TransformsValue()
    {
        // Arrange
        var value = 42;
        var maybe = value.AsMaybe();

        // Act
        var result = maybe.Map(x => x.ToString());

        // Assert
        result.Result.Should().Be("42");
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Map_WhenError_PropagatesError()
    {
        // Arrange
        var maybe = MaybeExtensions.Cancelled<int>();

        // Act
        var result = maybe.Map(x => x.ToString());

        // Assert
        result.Error.Should().NotBeNull();
        result.Error!.Value.Type.Should().Be(ErrorType.Canceled);
    }

    [Fact]
    public void Merge_WhenBothSuccessful_CombinesResults()
    {
        // Arrange
        var first = 42.AsMaybe();
        var second = "test".AsMaybe();

        // Act
        var result = first.Merge(second);

        // Assert
        result.Result.Should().Be((42, "test"));
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Merge_WhenOneHasError_CombinesErrors()
    {
        // Arrange
        var first = 42.AsMaybe();
        var second = MaybeExtensions.Cancelled<string>();

        // Act
        var result = first.Merge(second);

        // Assert
        result.Error.Should().NotBeNull();
        result.Error!.Value.Type.Should().Be(ErrorType.Canceled);
    }

    [Fact]
    public void Merge_WhenBothHaveErrors_CombinesAllErrors()
    {
        // Arrange
        var first = MaybeExtensions.Cancelled<int>();
        var second = MaybeExtensions.Cancelled<string>();

        // Act
        var result = first.Merge(second);

        // Assert
        result.Error.Should().NotBeNull();
        result.Error!.Value.Type.Should().Be(ErrorType.Canceled);
        result.Error!.Value.ErrorMessages.Should().HaveCount(1);
    }

    [Fact]
    public async Task AcceptAsync_WhenCancelled_ReturnsCancelledResult()
    {
        // Arrange
        var maybe = 42.AsMaybe();
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var result = await maybe.AcceptAsync((_, _) => Task.FromResult("test".AsMaybe()), _logger, cts.Token);

        // Assert
        result.Error.Should().NotBeNull();
        result.Error!.Value.Type.Should().Be(ErrorType.Canceled);
    }

    [Fact]
    public async Task AcceptAsync_WhenInputHasError_PropagatesError()
    {
        // Arrange
        var maybe = MaybeExtensions.Cancelled<int>();

        // Act
        var result = await maybe.AcceptAsync((_, _) => Task.FromResult("test".AsMaybe()), _logger, CancellationToken.None);

        // Assert
        result.Error.Should().NotBeNull();
        result.Error!.Value.Type.Should().Be(ErrorType.Canceled);
    }

    [Fact]
    public async Task AcceptAsync_WhenSuccessful_TransformsValue()
    {
        // Arrange
        var maybe = 42.AsMaybe();

        // Act
        var result = await maybe.AcceptAsync((x, _) => Task.FromResult(x.ToString().AsMaybe()), _logger, CancellationToken.None);

        // Assert
        result.Result.Should().Be("42");
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Accept_WhenInputHasError_PropagatesError()
    {
        // Arrange
        var maybe = MaybeExtensions.Cancelled<int>();

        // Act
        var result = maybe.Accept(x => x.ToString().AsMaybe(), _logger);

        // Assert
        result.Error.Should().NotBeNull();
        result.Error!.Value.Type.Should().Be(ErrorType.Canceled);
    }

    [Fact]
    public void Accept_WhenSuccessful_TransformsValue()
    {
        // Arrange
        var maybe = 42.AsMaybe();

        // Act
        var result = maybe.Accept(x => x.ToString().AsMaybe(), _logger);

        // Assert
        result.Result.Should().Be("42");
        result.Error.Should().BeNull();
    }
}
