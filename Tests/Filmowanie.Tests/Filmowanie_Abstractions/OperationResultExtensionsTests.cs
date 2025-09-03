using System.Text;
using AutoFixture;
using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Filmowanie.Tests.Filmowanie_Abstractions;

public sealed class OperationResultExtensionsTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void CancelledOperation_ShouldReturnCancelledError()
    {
        // Act
        var result = OperationResultExtensions.CancelledOperation<int>();

        // Assert
        result.Error.Should().NotBeNull();
        result.Error!.Value.ErrorMessages.Should().BeEquivalentTo("Operation canceled.");
        result.Error.Value.Type.Should().Be(ErrorType.Canceled);
        result.Result.Should().Be(default(int));
    }

    [Fact]
    public void Pluck_ShouldReturnError_WhenOperationResultHasError()
    {
        // Arrange
        var errorType = _fixture.Create<ErrorType>();
        var error = new Error("Some error", errorType);
        var operationResult = new OperationResult<StringBuilder>(new StringBuilder(), error);

        // Act
        var result = operationResult.Pluck(x => x.Length);

        // Assert
        result.Error!.Value.Should().Be(error);
    }

    [Fact]
    public void Pluck_ShouldTransformResult_WhenOperationResultHasNoError()
    {
        // Arrange
        var operationResult = new OperationResult<int>(42, null);

        // Act
        var result = operationResult.Pluck(x => x.ToString());

        // Assert
        result.Error.Should().BeNull();
        result.Result.Should().Be("42");
    }

    [Fact]
    public void ToOperationResult_ShouldReturnOperationResultWithNoError()
    {
        // Arrange
        var value = _fixture.Create<int>();

        // Act
        var result = value.ToOperationResult();

        // Assert
        result.Error.Should().BeNull();
        result.Result.Should().Be(value);
    }

    [Fact]
    public void Empty_ShouldReturnOperationResultWithNullError()
    {
        // Act
        var result = OperationResultExtensions.Empty;

        // Assert
        result.Error.Should().BeNull();
        result.Result.Should().Be(default);
    }

    [Fact]
    public async Task AcceptAsync_ShouldReturnError_WhenOperationResultCancelled_TwoGenericTypesOverload()
    {
        // Arrange
        var errorType = _fixture.Create<ErrorType>();
        var error = new Error("Some error", errorType);
        var operationResult = new OperationResult<int>(default, error);
        Func<int, CancellationToken, Task<string>> inlineVisitor = (_, _) => Task.FromResult("whatever");

        // Act
        var result = await operationResult.AcceptAsync(inlineVisitor, new CancellationToken(true));

        // Assert
        result.Error!.Value.ErrorMessages.Should().BeEquivalentTo("Some error", "Operation canceled.");
    }

    [Fact]
    public async Task AcceptAsync_ShouldReturnError_WhenOperationResultHasError_TwoGenericTypesOverload()
    {
        // Arrange
        var errorType = _fixture.Create<ErrorType>();
        var error = new Error("Some error", errorType);
        var operationResult = new OperationResult<int>(default, error);
        Func<int, CancellationToken, Task<string>> inlineVisitor = (_, _) => Task.FromResult("whatever");

        // Act
        var result = await operationResult.AcceptAsync(inlineVisitor, CancellationToken.None);

        // Assert
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task AcceptAsync_ShouldReturnTransformedResult_WhenOperationResultHasNoError_TwoGenericTypesOverload()
    {
        // Arrange
        var operationResult = new OperationResult<int>(42, null);
        Func<int, CancellationToken, Task<string>> inlineVisitor = (_, _) => Task.FromResult("Transformed");

        // Act
        var result = await operationResult.AcceptAsync(inlineVisitor, CancellationToken.None);

        // Assert
        result.Error.Should().BeNull();
        result.Result.Should().Be("Transformed");
    }

    [Fact]
    public async Task AcceptAsync_ShouldReturnError_WhenOperationResultCancelled()
    {
        // Arrange
        var errorType = _fixture.Create<ErrorType>();
        var error = new Error("Some error", errorType);
        var operationResult = new OperationResult<int>(default, error);
        Func<int, CancellationToken, Task> inlineVisitor = (_, _) => Task.CompletedTask;

        // Act
        var result = await operationResult.AcceptAsync(inlineVisitor, new CancellationToken(true));

        // Assert
        result.Error!.Value.ErrorMessages.Should().BeEquivalentTo("Some error", "Operation canceled.");
    }

    [Fact]
    public async Task AcceptAsync_ShouldReturnError_WhenOperationResultHasError()
    {
        // Arrange
        var errorType = _fixture.Create<ErrorType>();
        var error = new Error("Some error", errorType);
        var operationResult = new OperationResult<int>(default, error);
        Func<int, CancellationToken, Task> inlineVisitor = (_, _) => Task.CompletedTask;

        // Act
        var result = await operationResult.AcceptAsync(inlineVisitor, CancellationToken.None);

        // Assert
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task AcceptAsync_ShouldReturnTransformedResult_WhenOperationResultHasNoError()
    {
        // Arrange
        var operationResult = new OperationResult<int>(42, null);
        Func<int, CancellationToken, Task> inlineVisitor = (_, _) => Task.CompletedTask;

        // Act
        var result = await operationResult.AcceptAsync(inlineVisitor, CancellationToken.None);

        // Assert
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Merge_ShouldReturnCombinedError_WhenBothOperationResultHaveErrors()
    {
        // Arrange
        var firstErrorType = _fixture.Create<ErrorType>();
        var secondErrorType = _fixture.Create<ErrorType>();
        var expectedErrorType = firstErrorType < secondErrorType ?  secondErrorType : firstErrorType;

        var firstError = new Error(["First error", "First error B"], firstErrorType);
        var secondError = new Error(["Second error"], secondErrorType);
        var firstResult = new OperationResult<int>(default, firstError);
        var secondResult = new OperationResult<string>(default, secondError);

        // Act
        var result = firstResult.Merge(secondResult);

        // Assert
        result.Error.Should().NotBeNull();
        result.Error!.Value.ErrorMessages.Should().Contain(["First error", "First error B", "Second error"]);
        result.Error.Value.Type.Should().Be(expectedErrorType);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Merge_ShouldReturnCombinedError_WhenSingleOperationResultHasErrors(int erroneousOperation)
    {
        // Arrange
        var errorType = _fixture.Create<ErrorType>();

        var error = new Error(["??? error"], errorType);
        var firstResult = erroneousOperation == 1 ? new OperationResult<int>(default, error) : new OperationResult<int>(2137, null);
        var secondResult = erroneousOperation == 2 ? new OperationResult<int>(default, error) : new OperationResult<int>(2137, null);

        // Act
        var result = firstResult.Merge(secondResult);

        // Assert
        result.Error.Should().NotBeNull();
        result.Error!.Value.ErrorMessages.Should().Contain(["??? error"]);
        result.Error.Value.Type.Should().Be(errorType);
    }

    [Fact]
    public void Merge_ShouldReturnCombinedResult_WhenNeitherOperationResultHasError()
    {
        // Arrange
        var firstResult = new OperationResult<int>(42, null);
        var secondResult = new OperationResult<string>("Success", null);

        // Act
        var result = firstResult.Merge(secondResult);

        // Assert
        result.Error.Should().BeNull();
        result.Result.Should().Be((42, "Success"));
    }

    [Fact]
    public void Flatten_ShouldReturnFlattenedResult_WhenOperationResultHasNoError()
    {
        // Arrange
        var nestedResult = new OperationResult<((int, string), bool)>(((42, "Nested"), true), null);

        // Act
        var result = nestedResult.Flatten();

        // Assert
        result.Error.Should().BeNull();
        result.Result.Should().Be((42, "Nested", true));
    }

    [Fact]
    public void Flatten_ShouldReturnFlattenedError_WhenOperationResultHasError()
    {
        // Arrange
        var errorType = _fixture.Create<ErrorType>();

        var error = new Error(["Flatten error"], errorType);
        var nestedResult = new OperationResult<((int, string), bool)>(((default, default)!, default), error);

        // Act
        var result = nestedResult.Flatten();

        // Assert
        result.Error.Should().Be(error);
        result.Result.Should().Be((default, default, default)!);
    }

    [Fact]
    public async Task AcceptAsync_ShouldReturnCancelledOperation_WhenCancellationIsRequested_TwoGenericTypesOverload()
    {
        // Arrange
        var operation = new OperationResult<int>(42, null);
        var visitor = Substitute.For<IOperationAsyncVisitor<int, string>>();
        var cancellationToken = new CancellationToken(true);

        // Act
        var result = await operation.AcceptAsync(visitor, cancellationToken);

        // Assert
        result.Error.Should().NotBeNull();
        result.Error!.Value.ErrorMessages.Should().BeEquivalentTo("Operation canceled.");
        result.Error.Value.Type.Should().Be(ErrorType.Canceled);
        result.Result.Should().Be(default);
    }

    [Fact]
    public async Task AcceptAsync_ShouldReturnCancelledOperation_WhenCancellationIsRequestedAndErroneousOperation_TwoGenericTypesOverload()
    {
        // Arrange
        var operation = new OperationResult<int>(42, new Error("some error", ErrorType.AuthenticationIssue));
        var visitor = Substitute.For<IOperationAsyncVisitor<int, string>>();
        var cancellationToken = new CancellationToken(true);

        // Act
        var result = await operation.AcceptAsync(visitor, cancellationToken);

        // Assert
        result.Error.Should().NotBeNull();
        result.Error!.Value.ErrorMessages.Should().BeEquivalentTo("Operation canceled.", "some error");
        result.Error.Value.Type.Should().Be(ErrorType.AuthenticationIssue);
        result.Result.Should().Be(default);
    }

    [Fact]
    public async Task AcceptAsync_ShouldReturnError_WhenOperationHasError_TwoGenericTypesOverload()
    {
        // Arrange
        var errorType = _fixture.Create<ErrorType>();
        var error = new Error("Some error", errorType);
        var operation = new OperationResult<int>(default, error);
        var visitor = Substitute.For<IOperationAsyncVisitor<int, string>>();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await operation.AcceptAsync(visitor, cancellationToken);

        // Assert
        result.Error.Should().Be(error);
        result.Result.Should().Be(default);
    }

    [Fact]
    public async Task AcceptAsync_ShouldReturnVisitorResult_WhenOperationHasNoError_TwoGenericTypesOverload()
    {
        // Arrange
        var operation = new OperationResult<int>(42, null);
        var visitor = Substitute.For<IOperationAsyncVisitor<int, string>>();
        visitor.SignUp(operation, Arg.Any<CancellationToken>()).Returns(Task.FromResult(new OperationResult<string>("Success", null)));
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await operation.AcceptAsync(visitor, cancellationToken);

        // Assert
        result.Error.Should().BeNull();
        result.Result.Should().Be("Success");
    }

    [Fact]
    public async Task AcceptAsync_ShouldLogOperationAndResult_TwoGenericTypesOverload()
    {
        // Arrange
        var operation = new OperationResult<int>(42, null);
        var visitor = Substitute.For<IOperationAsyncVisitor<int, string>>();
        var log = new LoggerForTests<IOperationAsyncVisitor<int, string>>();
        visitor.Log.Returns(log);
        visitor.SignUp(operation, Arg.Any<CancellationToken>()).Returns(Task.FromResult(new OperationResult<string>("Success", null)));
        var cancellationToken = CancellationToken.None;

        // Act
        _ = await operation.AcceptAsync(visitor, cancellationToken);

        // Assert
        log.Received(LogLevel.Information, "Visiting");
        log.Received(LogLevel.Information, "Concluded");
        log.TotalReceived.Should().Be(2);
    }

    [Fact]
    public async Task AcceptAsync_ShouldLogError_WhenVisitorReturnsError_TwoGenericTypesOverload()
    {
        // Arrange
        var errorType = _fixture.Create<ErrorType>();
        var operation = new OperationResult<int>(42, null);
        var error = new Error("Visitor error", errorType);
        var log = new LoggerForTests<IOperationAsyncVisitor<int, string>>();
        var visitor = Substitute.For<IOperationAsyncVisitor<int, string>>();
        visitor.Log.Returns(log);
        visitor.SignUp(operation, Arg.Any<CancellationToken>()).Returns(Task.FromResult(new OperationResult<string>(default, error)));
        var cancellationToken = CancellationToken.None;

        // Act
        _ = await operation.AcceptAsync(visitor, cancellationToken);

        // Assert
        log.Received(LogLevel.Information, "Visiting");
        log.Received(LogLevel.Error, "ERROR!!");
        log.TotalReceived.Should().Be(2);
    }

    [Fact]
    public async Task AcceptAsync_ShouldReturnCancelledOperation_WhenCancellationIsRequested()
    {
        // Arrange
        var operation = new OperationResult<int>(42, null);
        var visitor = Substitute.For<IOperationAsyncVisitor<string>>();
        var cancellationToken = new CancellationToken(true);

        // Act
        var result = await operation.AcceptAsync(visitor, cancellationToken);

        // Assert
        result.Error.Should().NotBeNull();
        result.Error!.Value.ErrorMessages.Should().BeEquivalentTo("Operation canceled.");
        result.Error.Value.Type.Should().Be(ErrorType.Canceled);
        result.Result.Should().Be(default);
    }

    [Fact]
    public async Task AcceptAsync_ShouldReturnCancelledOperation_WhenCancellationIsRequestedAndErroneousOperation()
    {
        // Arrange
        var operation = new OperationResult<int>(42, new Error("some error", ErrorType.AuthenticationIssue));
        var visitor = Substitute.For<IOperationAsyncVisitor<string>>();
        var cancellationToken = new CancellationToken(true);

        // Act
        var result = await operation.AcceptAsync(visitor, cancellationToken);

        // Assert
        result.Error.Should().NotBeNull();
        result.Error!.Value.ErrorMessages.Should().BeEquivalentTo("Operation canceled.", "some error");
        result.Error.Value.Type.Should().Be(ErrorType.AuthenticationIssue);
        result.Result.Should().Be(default);
    }

    [Fact]
    public async Task AcceptAsync_ShouldReturnError_WhenOperationHasError()
    {
        // Arrange
        var errorType = _fixture.Create<ErrorType>();
        var error = new Error("Some error", errorType);
        var operation = new OperationResult<int>(default, error);
        var visitor = Substitute.For<IOperationAsyncVisitor<string>>();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await operation.AcceptAsync(visitor, cancellationToken);

        // Assert
        result.Error.Should().Be(error);
        result.Result.Should().Be(default);
    }

    [Fact]
    public async Task AcceptAsync_ShouldReturnVisitorResult_WhenOperationHasNoError()
    {
        // Arrange
        var operation = new OperationResult<int>(42, null);
        var visitor = Substitute.For<IOperationAsyncVisitor<string>>();
        visitor.VisitAsync(operation, Arg.Any<CancellationToken>()).Returns(Task.FromResult(new OperationResult<string>("Success", null)));
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await operation.AcceptAsync(visitor, cancellationToken);

        // Assert
        result.Error.Should().BeNull();
        result.Result.Should().Be("Success");
    }

    [Fact]
    public async Task AcceptAsync_ShouldLogOperationAndResult()
    {
        // Arrange
        var operation = new OperationResult<int>(42, null);
        var visitor = Substitute.For<IOperationAsyncVisitor<int, string>>();
        var log = new LoggerForTests<IOperationAsyncVisitor<string>>();
        visitor.Log.Returns(log);
        visitor.SignUp(operation, Arg.Any<CancellationToken>()).Returns(Task.FromResult(new OperationResult<string>("Success", null)));
        var cancellationToken = CancellationToken.None;

        // Act
        _ = await operation.AcceptAsync(visitor, cancellationToken);

        // Assert
        log.Received(LogLevel.Information, "Visiting");
        log.Received(LogLevel.Information, "Concluded");
        log.TotalReceived.Should().Be(2);
    }

    [Fact]
    public async Task AcceptAsync_ShouldLogError_WhenVisitorReturnsError()
    {
        // Arrange
        var errorType = _fixture.Create<ErrorType>();
        var operation = new OperationResult<int>(42, null);
        var error = new Error("Visitor error", errorType);
        var log = new LoggerForTests<IOperationAsyncVisitor<int, string>>();
        var visitor = Substitute.For<IOperationAsyncVisitor<string>>();
        visitor.Log.Returns(log);
        visitor.VisitAsync(operation, Arg.Any<CancellationToken>()).Returns(Task.FromResult(new OperationResult<string>(default, error)));
        var cancellationToken = CancellationToken.None;

        // Act
        _ = await operation.AcceptAsync(visitor, cancellationToken);

        // Assert
        log.Received(LogLevel.Information, "Visiting");
        log.Received(LogLevel.Error, "ERROR!!");
        log.TotalReceived.Should().Be(2);
    }

    [Fact]
    public void Accept_ShouldReturnError_WhenOperationHasError_TwoGenericTypesOverload()
    {
        // Arrange
        var errorType = _fixture.Create<ErrorType>();
        var error = new Error("Some error", errorType);
        var operation = new OperationResult<int>(default, error);
        var visitor = Substitute.For<IOperationVisitor<int, string>>();

        // Act
        var result = operation.Accept(visitor);

        // Assert
        result.Error.Should().Be(error);
        result.Result.Should().Be(default);
    }

    [Fact]
    public void Accept_ShouldReturnVisitorResult_WhenOperationHasNoError_TwoGenericTypesOverload()
    {
        // Arrange
        var operation = new OperationResult<int>(42, null);
        var visitor = Substitute.For<IOperationVisitor<int, string>>();
        visitor.Visit(operation).Returns(new OperationResult<string>("Success", null));

        // Act
        var result = operation.Accept(visitor);

        // Assert
        result.Error.Should().BeNull();
        result.Result.Should().Be("Success");
    }

    [Fact]
    public void Accept_ShouldLogOperationAndResult_TwoGenericTypesOverload()
    {
        // Arrange
        var operation = new OperationResult<int>(42, null);
        var visitor = Substitute.For<IOperationVisitor<int, string>>();
        var log = new LoggerForTests<IOperationVisitor<int, string>>();
        visitor.Log.Returns(log);
        visitor.Visit(operation).Returns(new OperationResult<string>("Success", null));

        // Act
        _ = operation.Accept(visitor);

        // Assert
        log.Received(LogLevel.Information, "Visiting");
        log.Received(LogLevel.Information, "Concluded");
        log.TotalReceived.Should().Be(2);
    }

    [Fact]
    public void Accept_ShouldLogError_WhenVisitorReturnsError_TwoGenericTypesOverload()
    {
        // Arrange
        var errorType = _fixture.Create<ErrorType>();
        var operation = new OperationResult<int>(42, null);
        var error = new Error("Visitor error", errorType);
        var log = new LoggerForTests<IOperationAsyncVisitor<int, string>>();
        var visitor = Substitute.For<IOperationAsyncVisitor<int, string>>();
        visitor.Log.Returns(log);
        visitor.SignUp(operation, Arg.Any<CancellationToken>()).Returns(Task.FromResult(new OperationResult<string>(default, error)));
        var cancellationToken = CancellationToken.None;

        // Act
        _ =  operation.AcceptAsync(visitor, cancellationToken);

        // Assert
        log.Received(LogLevel.Information, "Visiting");
        log.Received(LogLevel.Error, "ERROR!!");
        log.TotalReceived.Should().Be(2);
    }

    [Fact]
    public void Accept_ShouldReturnError_WhenOperationHasError()
    {
        // Arrange
        var errorType = _fixture.Create<ErrorType>();
        var error = new Error("Some error", errorType);
        var operation = new OperationResult<int>(default, error);
        var visitor = Substitute.For<IOperationVisitor<string>>();

        // Act
        var result = operation.Accept(visitor);

        // Assert
        result.Error.Should().Be(error);
        result.Result.Should().Be(default);
    }

    [Fact]
    public void Accept_ShouldReturnVisitorResult_WhenOperationHasNoError()
    {
        // Arrange
        var operation = new OperationResult<int>(42, null);
        var visitor = Substitute.For<IOperationVisitor<string>>();
        visitor.Visit(operation).Returns(new OperationResult<string>("Success", null));

        // Act
        var result = operation.Accept(visitor);

        // Assert
        result.Error.Should().BeNull();
        result.Result.Should().Be("Success");
    }

    [Fact]
    public void Accept_ShouldLogOperationAndResult()
    {
        // Arrange
        var operation = new OperationResult<int>(42, null);
        var visitor = Substitute.For<IOperationVisitor<string>>();
        var log = new LoggerForTests<IOperationVisitor<string>>();
        visitor.Log.Returns(log);
        visitor.Visit(operation).Returns(new OperationResult<string>("Success", null));

        // Act
        _ = operation.Accept(visitor);

        // Assert
        log.Received(LogLevel.Information, "Visiting");
        log.Received(LogLevel.Information, "Concluded");
        log.TotalReceived.Should().Be(2);
    }

    [Fact]
    public void Accept_ShouldLogError_WhenVisitorReturnsError()
    {
        // Arrange
        var errorType = _fixture.Create<ErrorType>();
        var operation = new OperationResult<int>(42, null);
        var error = new Error("Visitor error", errorType);
        var log = new LoggerForTests<IOperationAsyncVisitor<string>>();
        var visitor = Substitute.For<IOperationAsyncVisitor<string>>();
        visitor.Log.Returns(log);
        visitor.VisitAsync(operation, Arg.Any<CancellationToken>()).Returns(Task.FromResult(new OperationResult<string>(default, error)));
        var cancellationToken = CancellationToken.None;

        // Act
        _ = operation.AcceptAsync(visitor, cancellationToken);

        // Assert
        log.Received(LogLevel.Information, "Visiting");
        log.Received(LogLevel.Error, "ERROR!!");
        log.TotalReceived.Should().Be(2);
    }
}