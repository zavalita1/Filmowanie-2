using System.Security.Claims;
using Filmowanie.Account.Constants;
using Filmowanie.Filters;
using Filmowanie.UnitTests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Filmowanie.UnitTests.Filters;

public sealed class LoggingActionFilterTests
{
    private readonly LoggerForTests<LoggingActionFilter> _mockLogger;
    private readonly LoggingActionFilter _sut;

    public LoggingActionFilterTests()
    {
        _mockLogger = new LoggerForTests<LoggingActionFilter>();
        _sut = new LoggingActionFilter(_mockLogger);
    }

    [Fact]
    public async Task InvokeAsync_LogsRequestStartingAndEnding()
    {
        // Arrange
        var context = new DefaultHttpContext
        {
            Request =
            {
                Path = "/test-path"
            },
            User = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimsTypes.UserId, "test-user-id")]))
        };

        var invocationContext = new EndpointFilterInvocationContextForTests(context);
        var returnObj = (object?)new ();
        var next = (EndpointFilterDelegate) (_ => ValueTask.FromResult(returnObj));

        // Act
        var result = await _sut.InvokeAsync(invocationContext, next);

        // Assert
        result.Should().Be(returnObj);
        _mockLogger.Received(LogLevel.Information, "Request starting").Should().BeTrue();
        _mockLogger.Received(LogLevel.Information, "Request ending").Should().BeTrue();
        _mockLogger.TotalReceived.Should().Be(2);
    }

    [Fact]
    public async Task InvokeAsync_LogsRequestStartingAndEndingAtProperTime()
    {
        // Arrange
        var context = new DefaultHttpContext
        {
            Request =
            {
                Path = "/test-path"
            },
            User = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimsTypes.UserId, "test-user-id")]))
        };

        var invocationContext = new EndpointFilterInvocationContextForTests(context);
        var manual1 = new ManualResetEventSlim(false);
        var manual2 = new ManualResetEventSlim(false);
        var next = (EndpointFilterDelegate)(_ =>
        {
            manual1.Set();
            manual2.Wait();
            return ValueTask.FromResult((object?)new());
        });

        // Act
        // Assert
        var task = Task.Run(() => _sut.InvokeAsync(invocationContext, next));
        manual1.Wait(TimeSpan.FromSeconds(5));

        _mockLogger.Received(LogLevel.Information, "Request starting").Should().BeTrue();
        _mockLogger.TotalReceived.Should().Be(1);

        manual2.Set();
        await task;
        _mockLogger.Received(LogLevel.Information, "Request ending").Should().BeTrue();
        _mockLogger.TotalReceived.Should().Be(2);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsNextResult()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var invocationContext = new EndpointFilterInvocationContextForTests(context);
        var expectedResult = new object();
        var next = Substitute.For<EndpointFilterDelegate>();
        next.Invoke(Arg.Any<EndpointFilterInvocationContextForTests>()).Returns(expectedResult);

        // Act
        var result = await _sut.InvokeAsync(invocationContext, next);

        // Assert
        result.Should().Be(expectedResult);
    }

    private class EndpointFilterInvocationContextForTests : EndpointFilterInvocationContext
    {
        public EndpointFilterInvocationContextForTests(HttpContext context)
        {
            HttpContext = context;
            Arguments = [];
        }

        public override T GetArgument<T>(int index) => throw new NotImplementedException();

        public override HttpContext HttpContext { get; }

        public override IList<object?> Arguments { get; }
    }
}