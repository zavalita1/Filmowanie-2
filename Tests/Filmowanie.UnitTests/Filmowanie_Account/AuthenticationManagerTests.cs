using System.Globalization;
using System.Security.Claims;
using System.Security.Principal;
using AutoFixture;
using Filmowanie.Abstractions.Constants;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.Constants;
using Filmowanie.Account.Interfaces;
using Filmowanie.Account.Models;
using Filmowanie.Account.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Filmowanie.UnitTests.Filmowanie_Account;

public sealed class AuthenticationManagerTests
{
    private readonly IHttpContextWrapper _httpContextWrapper;
    private readonly AuthenticationManager _sut;

    private readonly Fixture _fixture = new();

    public AuthenticationManagerTests()
    {
        var logger = Substitute.For<ILogger<AuthenticationManager>>();
        _httpContextWrapper = Substitute.For<IHttpContextWrapper>();
        _sut = new AuthenticationManager(logger, _httpContextWrapper);
    }

    [Fact]
    public async Task LogInAsync_WithValidData_SignsInUser()
    {
        // Arrange
        var claimsIdentity = new ClaimsIdentity([new Claim("test", "value")], Schemes.Cookie);
        var authProps = new AuthenticationProperties();
        var loginData = new LoginResultData(claimsIdentity, authProps);
        var input = new Maybe<LoginResultData>(loginData, null);

        // Act
        var result = await _sut.LogInAsync(input, CancellationToken.None);

        // Assert
        result.Result.Should().NotBeNull();
        result.Error.Should().BeNull();

        await _httpContextWrapper
            .Received(1)
            .SignInAsync(
                Schemes.Cookie,
                Arg.Is<ClaimsPrincipal>(cp => cp.Identity == claimsIdentity),
                Arg.Is<AuthenticationProperties>(ap => ap == authProps));
    }

    [Fact]
    public async Task LogOutAsync_SignsOutUser()
    {
        // Arrange
        var input = VoidResult.Void;

        // Act
        var result = await _sut.LogOutAsync(input, CancellationToken.None);

        // Assert
        result.Result.Should().NotBeNull();
        result.Error.Should().BeNull();

        await _httpContextWrapper
            .Received(1)
            .SignOutAsync(Schemes.Cookie);
    }

    [Fact]
    public void GetDomainUser_WhenUserIsAuthenticatedWithValidClaims_ReturnsDomainUser()
    {
        // Arrange
        var userId = "user-2137";
        var username = "Mr Bean";
        var isAdmin = true;
        var hasBasicAuth = true;
        var tenantId = 42;
        var created = DateTime.UtcNow;
        var gender = _fixture.Create<Gender>();

        var claims = new[]
        {
            new Claim(ClaimsTypes.UserId, userId),
            new Claim(ClaimsTypes.UserName, username),
            new Claim(ClaimsTypes.IsAdmin, isAdmin.ToString()),
            new Claim(ClaimsTypes.HasBasicAuth, hasBasicAuth.ToString()),
            new Claim(ClaimsTypes.Tenant, tenantId.ToString(CultureInfo.InvariantCulture)),
            new Claim(ClaimsTypes.Created, created.ToString("O")),
            new Claim(ClaimsTypes.Gender, gender.ToString())
        };

        var identity = Substitute.For<IIdentity>();
        identity.IsAuthenticated.Returns(true);
        var principal = Substitute.For<ClaimsPrincipal>();
        principal.Claims.Returns(claims);
        principal.Identity.Returns(identity);
        _httpContextWrapper.User.Returns(principal);

        var input = VoidResult.Void;

        // Act
        var result = _sut.GetDomainUser(input);

        // Assert
        result.Result.Should().NotBeNull();
        result.Error.Should().BeNull();

        var user = result.Result;
        user.Should().NotBeNull();
        user!.Id.Should().Be(userId);
        user.Name.Should().Be(username);
        user.IsAdmin.Should().Be(isAdmin);
        user.HasBasicAuthSetup.Should().Be(hasBasicAuth);
        user.Tenant.Id.Should().Be(tenantId);
        user.Created.Should().Be(created);
        user.Gender.Should().Be(gender);
    }

    [Fact]
    public void GetDomainUser_WhenUserIsNotAuthenticated_ReturnsError()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        _httpContextWrapper.User.Returns(principal);
        var input = VoidResult.Void;

        // Act
        var result = _sut.GetDomainUser(input);

        // Assert
        result.Result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Value.Type.Should().Be(ErrorType.AuthenticationIssue);
        result.Error!.ToString().Should().Contain(Messages.UserNotLoggerIn);
    }

    [Fact]
    public void GetDomainUser_WhenCookieExpired_ReturnsErrorWithBothMessages()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        _httpContextWrapper.User.Returns(principal);
        _httpContextWrapper.Request!.Cookies.ContainsKey(".AspNetCore.cookie").Returns(true);
        var input = VoidResult.Void;

        // Act
        var result = _sut.GetDomainUser(input);

        // Assert
        result.Result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Value.Type.Should().Be(ErrorType.AuthenticationIssue);
        result.Error!.ToString().Should().Contain(Messages.CookieExpired);
        result.Error!.ToString().Should().Contain(Messages.UserNotLoggerIn);
    }

    [Fact]
    public void GetDomainUser_WhenUserIsNull_ReturnsError()
    {
        // Arrange
        _httpContextWrapper.User.Returns((ClaimsPrincipal?)null);
        var input = VoidResult.Void;

        // Act
        var result = _sut.GetDomainUser(input);

        // Assert
        result.Result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Value.Type.Should().Be(ErrorType.AuthenticationIssue);
        result.Error!.ToString().Should().Contain(Messages.UserNotLoggerIn);
    }

    [Fact]
    public void GetDomainUser_WhenMissingRequiredClaim_ThrowsInvalidOperationException()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimsTypes.UserId, "user-2137"),
            // Missing UserName claim
            new Claim(ClaimsTypes.IsAdmin, "true"),
            new Claim(ClaimsTypes.HasBasicAuth, "true"),
            new Claim(ClaimsTypes.Tenant, "42"),
            new Claim(ClaimsTypes.Created, DateTime.UtcNow.ToString("O"))
        };

        var identity = Substitute.For<IIdentity>();
        identity.IsAuthenticated.Returns(true);
        var principal = Substitute.For<ClaimsPrincipal>();
        principal.Claims.Returns(claims);
        principal.Identity.Returns(identity);
        _httpContextWrapper.User.Returns(principal);

        var input = VoidResult.Void;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _sut.GetDomainUser(input));
    }
}
