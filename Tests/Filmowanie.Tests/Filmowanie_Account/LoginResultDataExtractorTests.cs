using System.Globalization;
using AutoFixture;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Account.Constants;
using Filmowanie.Account.Helpers;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using FluentAssertions;
using NSubstitute;

namespace Filmowanie.Tests.Filmowanie_Account;

public sealed class LoginResultDataExtractorTests
{
    private readonly IFixture _fixture;
    private readonly LoginResultDataExtractor _sut;
    private readonly IReadOnlyUserEntity _userEntity;

    public LoginResultDataExtractorTests()
    {
        _fixture = new Fixture();
        _sut = new LoginResultDataExtractor();
        _userEntity = Substitute.For<IReadOnlyUserEntity>();
    }

    [Fact]
    public void GetIdentity_WhenUserHasPasswordHash_ReturnsLoginResultDataWithCorrectClaims()
    {
        // Arrange
        var displayName = "Mr Bean";
        var userId = "2137";
        var isAdmin = true;
        var tenantId = 42;
        var created = DateTime.UtcNow;
        var passwordHash = "somehash";
        var gender = _fixture.Create<Gender>();

        _userEntity.DisplayName.Returns(displayName);
        _userEntity.id.Returns(userId);
        _userEntity.IsAdmin.Returns(isAdmin);
        _userEntity.TenantId.Returns(tenantId);
        _userEntity.Created.Returns(created);
        _userEntity.PasswordHash.Returns(passwordHash);

        // Act
        var result = _sut.GetIdentity(_userEntity);

        // Assert
        result.Result.Should().NotBeNull();
        result.Result!.Identity.Claims.Should().Contain(c => c.Type == ClaimsTypes.UserName && c.Value == displayName);
        result.Result!.Identity.Claims.Should().Contain(c => c.Type == ClaimsTypes.UserId && c.Value == userId);
        result.Result!.Identity.Claims.Should().Contain(c => c.Type == ClaimsTypes.IsAdmin && c.Value == isAdmin.ToString(CultureInfo.InvariantCulture));
        result.Result!.Identity.Claims.Should().Contain(c => c.Type == ClaimsTypes.Tenant && c.Value == tenantId.ToString());
        result.Result!.Identity.Claims.Should().Contain(c => c.Type == ClaimsTypes.HasBasicAuth && c.Value == true.ToString(CultureInfo.InvariantCulture));
        result.Result!.Identity.Claims.Should().Contain(c => c.Type == ClaimsTypes.Created && c.Value == created.ToString("O"));
        result.Result!.Identity.Claims.Should().Contain(c => c.Type == ClaimsTypes.Gender && c.Value == gender.ToString());
    }

    [Fact]
    public void GetIdentity_WhenUserHasNoPasswordHash_ReturnsLoginResultDataWithHasBasicAuthFalse()
    {
        // Arrange
        _userEntity.DisplayName.Returns(_fixture.Create<string>());
        _userEntity.id.Returns(_fixture.Create<string>());
        _userEntity.IsAdmin.Returns(_fixture.Create<bool>());
        _userEntity.TenantId.Returns(_fixture.Create<int>());
        _userEntity.Created.Returns(_fixture.Create<DateTime>());
        _userEntity.PasswordHash.Returns(string.Empty);

        // Act
        var result = _sut.GetIdentity(_userEntity);

        // Assert
        result.Result.Should().NotBeNull();
        result.Result!.Identity.Claims.Should().Contain(c => 
            c.Type == ClaimsTypes.HasBasicAuth && c.Value == false.ToString(CultureInfo.InvariantCulture));
    }

    [Fact]
    public void GetIdentity_ReturnsLoginResultDataWithCorrectAuthenticationProperties()
    {
        // Arrange
        _userEntity.DisplayName.Returns(_fixture.Create<string>());
        _userEntity.id.Returns(_fixture.Create<string>());
        _userEntity.IsAdmin.Returns(_fixture.Create<bool>());
        _userEntity.TenantId.Returns(_fixture.Create<int>());
        _userEntity.Created.Returns(_fixture.Create<DateTime>());

        // Act
        var result = _sut.GetIdentity(_userEntity);

        // Assert
        result.Result.Should().NotBeNull();
        result.Result!.AuthenticationProperties.AllowRefresh.Should().BeTrue();
        result.Result!.AuthenticationProperties.IsPersistent.Should().BeTrue();
        result.Result!.AuthenticationProperties.IssuedUtc.Should().NotBeNull();
        result.Result!.AuthenticationProperties.ExpiresUtc.Should().NotBeNull();
    }
}
