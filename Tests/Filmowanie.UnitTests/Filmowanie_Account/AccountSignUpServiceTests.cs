using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.Interfaces;
using Filmowanie.Account.Models;
using Filmowanie.Account.Services;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Filmowanie.UnitTests.Filmowanie_Account;

public sealed class AccountSignUpServiceTests
{
    private readonly IUsersCommandRepository _commandRepository;
    private readonly IHashHelper _hashHelper;
    private readonly ILoginResultDataExtractor _extractor;
    private readonly AccountSignUpService _sut;

    public AccountSignUpServiceTests()
    {
        _commandRepository = Substitute.For<IUsersCommandRepository>();
        _hashHelper = Substitute.For<IHashHelper>();
        var logger = Substitute.For<ILogger<AccountSignUpService>>();
        _extractor = Substitute.For<ILoginResultDataExtractor>();
        var httpContextWrapper = Substitute.For<IHttpContextWrapper>();
        _sut = new AccountSignUpService(_commandRepository, _hashHelper, logger, _extractor, httpContextWrapper);
    }

    [Fact]
    public async Task SignUp_WhenSuccessful_ReturnsLoginResultData()
    {
        // Arrange
        var userId = "user-2137";
        var created = DateTime.UtcNow;
        var password = "password123";
        var email = "mr.bean@atkinson.com";
        var expectedHash = "hashedPassword";
        var salt = userId + created.Minute;
        
        var domainUser = new DomainUser(userId, "ext-42", false, false, new TenantId(21), created, Gender.Unspecified);
        var basicAuth = new BasicAuthUserData(email, password);

        var userEntity = Substitute.For<IReadOnlyUserEntity>();
        var expectedLoginResult = new LoginResultData(null!, null!);

        _hashHelper.GetHash(password, salt).Returns(expectedHash);
        _commandRepository
            .UpdatePasswordAndMail(
                userId,
                Arg.Is<(string Mail, string Password)>(b => b.Mail == email && b.Password == expectedHash),
                Arg.Any<CancellationToken>())
            .Returns(userEntity);
        _extractor.GetIdentity(userEntity).Returns(new Maybe<LoginResultData>(expectedLoginResult, null));

        // Act
        var result = await _sut.SignUp(domainUser.AsMaybe(), basicAuth.AsMaybe(), CancellationToken.None);

        // Assert
        result.Result.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Result.Should().Be(expectedLoginResult);

        await _commandRepository
            .Received(1)
            .UpdatePasswordAndMail(
                userId,
                Arg.Is<(string Mail, string Password)>(b => b.Mail == email && b.Password == expectedHash),
                Arg.Any<CancellationToken>());

        _hashHelper.Received(1).GetHash(password, salt);
        _extractor.Received(1).GetIdentity(userEntity);
    }

    [Fact]
    public async Task SignUp_WhenInputHasError_ReturnsError()
    {
        // Arrange
        var error = new Error<DomainUser>("", ErrorType.InvalidState);

        // Act
        var result = await _sut.SignUp(error, default(BasicAuthUserData).AsMaybe(), CancellationToken.None);

        // Assert
        result.Result.Identity.Should().BeNull();
        result.Result.AuthenticationProperties.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Value.Type.Should().Be(ErrorType.InvalidState, null);

        await _commandRepository.DidNotReceive().UpdatePasswordAndMail(
            Arg.Any<string>(),
            Arg.Any<(string Mail, string Password)>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SignUp_WhenDatabaseUpdateFails_ReturnsInvalidStateError()
    {
        // Arrange
        var userId = "user-2137";
        var created = DateTime.UtcNow;
        var domainUser = new DomainUser(userId, "ext-42", false, false, new TenantId(21), created, Gender.Unspecified);
        var basicAuth = new BasicAuthUserData("mr.bean@atkinson.com", "password123");

        _commandRepository
            .UpdatePasswordAndMail(Arg.Any<string>(), Arg.Any<(string Mail, string Password)>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _sut.SignUp(domainUser.AsMaybe(), basicAuth.AsMaybe(), CancellationToken.None);

        // Assert
        result.Result.Identity.Should().BeNull();
        result.Result.AuthenticationProperties.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Value.Type.Should().Be(ErrorType.InvalidState);
        result.Error!.ToString().Should().Be("No user with such id in db");
    }
}
