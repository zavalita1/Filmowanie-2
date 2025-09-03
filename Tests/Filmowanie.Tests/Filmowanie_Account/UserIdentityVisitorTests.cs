using System.Globalization;
using System.Security.Claims;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.Constants;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Filmowanie.Tests.Filmowanie_Account;

public sealed class UserIdentityVisitorTests
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly UserIdentityVisitor _visitor;

    public UserIdentityVisitorTests()
    {
        _contextAccessor = Substitute.For<IHttpContextAccessor>();
        var log = Substitute.For<ILogger<UserIdentityVisitor>>();
        _visitor = new UserIdentityVisitor(_contextAccessor, log);
    }

    [Fact]
    public void Visit_UserNull_ReturnsAuthenticationError()
    {
        // Arrange
        var operationResult = new OperationResult<string>("someResult", null);
        var context = new DefaultHttpContext
        {
            User = null!
        };
        _contextAccessor.HttpContext.Returns(context);

        // Act
        var result = _visitor.Visit(operationResult);

        // Assert
        result.Error.Should().NotBeNull();
        result.Error!.Value.ErrorMessages.Should().Contain(Messages.UserNotLoggerIn);
        result.Error.Value.Type.Should().Be(ErrorType.AuthenticationIssue);
    }

    [Fact]
    public void Visit_UserAuthenticated_ReturnsDomainUser()
    {
        // Arrange
        var operationResult = new OperationResult<string>("someResult", null);
        var claims = new[]
        {
            new Claim(ClaimsTypes.UserId, "userId"),
            new Claim(ClaimsTypes.UserName, "username"),
            new Claim(ClaimsTypes.IsAdmin, "true"),
            new Claim(ClaimsTypes.HasBasicAuth, "true"),
            new Claim(ClaimsTypes.Tenant, "1"),
            new Claim(ClaimsTypes.Created, DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture))
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var user = new ClaimsPrincipal(identity);
        var context = new DefaultHttpContext { User = user };
        _contextAccessor.HttpContext.Returns(context);

        // Act
        var result = _visitor.Visit(operationResult);

        // Assert
        result.Result.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Result.Should().NotBeNull();
        result.Result.Id.Should().Be("userId");
        result.Result.IsAdmin.Should().BeTrue();
        result.Result.HasBasicAuthSetup.Should().BeTrue();
        result.Result.Tenant.Id.Should().Be(1);
        result.Result.Created.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}