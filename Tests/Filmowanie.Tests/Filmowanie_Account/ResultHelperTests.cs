using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.Constants;
using Filmowanie.Account.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Filmowanie.Tests.Filmowanie_Account;

public sealed class ResultHelperTests
{
    private readonly RoutesResultHelper _routesResultHelper = new();

    [Fact]
    public void RoutesResultHelperUnwrapOperationResult_ShouldReturnOk_WhenNoError()
    {
        // Arrange
        var result = new Maybe<string> { Result = "Success", Error = null };

        // Act
        var response = _routesResultHelper.UnwrapOperationResult(result);

        // Assert
        response.Should().BeOfType<Ok<string>>();
    }

    [Fact]
    public void RoutesResultHelperUnwrapOperationResult_ShouldReturnBadRequest_WhenIncomingDataIssue()
    {
        // Arrange
        var error = new Error<string> { Type = ErrorType.IncomingDataIssue, ErrorMessages = new List<string> { "Invalid data" } };
        var result = new Maybe<string> { Error = error };

        // Act
        var response = _routesResultHelper.UnwrapOperationResult(result);

        // Assert
        response.Should().BeOfType<BadRequest<string>>();
    }

    [Fact]
    public void RoutesResultHelperUnwrapOperationResult_ShouldReturnUnauthorized_WhenAuthenticationIssue()
    {
        // Arrange
        var error = new Error<string> { Type = ErrorType.AuthenticationIssue, ErrorMessages = new List<string> { "Authentication failed" } };
        var result = new Maybe<string> { Error = error };

        // Act
        var response = _routesResultHelper.UnwrapOperationResult(result);

        // Assert
        response.Should().BeOfType<ProblemHttpResult>();
        response.As<ProblemHttpResult>().ProblemDetails.Detail.Should().Be("Please log in");
    }

    [Fact]
    public void RoutesResultHelperUnwrapOperationResult_ShouldReturnProblem_WhenCookieExpired()
    {
        // Arrange
        var error = new Error<string> { Type = ErrorType.AuthenticationIssue, ErrorMessages = new List<string> { Messages.CookieExpired } };
        var result = new Maybe<string> { Error = error };

        // Act
        var response = _routesResultHelper.UnwrapOperationResult(result);

        // Assert
        response.Should().BeOfType<ProblemHttpResult>();
        response.As<ProblemHttpResult>().ProblemDetails.Detail.Should().Be("Cookie expired!");
    }

    [Fact]
    public void RoutesResultHelperUnwrapOperationResult_ShouldThrowInvalidOperationException_WhenUnhandledErrorType()
    {
        // Arrange
        var error = new Error<string> { Type = (ErrorType)999, ErrorMessages = new List<string> { "Unknown error" } };
        var result = new Maybe<string> { Error = error };

        // Act
        Action act = () => _routesResultHelper.UnwrapOperationResult(result);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Erroneous result! Unknown error.");
    }
}