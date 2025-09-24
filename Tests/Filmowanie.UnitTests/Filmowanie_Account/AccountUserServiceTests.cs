using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.Interfaces;
using Filmowanie.Account.Models;
using Filmowanie.Account.Services;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Filmowanie.UnitTests.Filmowanie_Account;

public sealed class AccountUserServiceTests
{
    private readonly IDomainUsersRepository _usersQueryRepository;
    private readonly IUsersCommandRepository _usersCommandRepository;
    private readonly IGuidProvider _guidProvider;
    private readonly ILoginResultDataExtractor _extractor;
    private readonly IHashHelper _hashHelper;
    private readonly AccountUserService _sut;

    public AccountUserServiceTests()
    {
        _usersQueryRepository = Substitute.For<IDomainUsersRepository>();
        _usersCommandRepository = Substitute.For<IUsersCommandRepository>();
        _guidProvider = Substitute.For<IGuidProvider>();
        var logger = Substitute.For<ILogger<AccountUserService>>();
        _extractor = Substitute.For<ILoginResultDataExtractor>();
        _hashHelper = Substitute.For<IHashHelper>();

        var adapterFactory = Substitute.For<ILoginDataExtractorAdapterFactory>();
        adapterFactory.GetExtractor().Returns(_extractor);

        _sut = new AccountUserService(
            _usersQueryRepository,
            logger,
            adapterFactory,
            _usersCommandRepository,
            _guidProvider);
    }

    [Fact]
    public async Task GetUserIdentity_WithValidCode_ReturnsLoginResultData()
    {
        // Arrange
        var code = "valid-code-2137";
        var input = new Code(code).AsMaybe();
        var user = Substitute.For<IReadOnlyUserEntity>();
        var expectedLoginResult = new LoginResultData(null!, null!);

        _usersQueryRepository.GetUserByCodeAsync(code, Arg.Any<CancellationToken>())
            .Returns(user);
        _extractor.GetIdentity(user)
            .Returns(new Maybe<LoginResultData>(expectedLoginResult, null));

        // Act
        var result = await _sut.GetUserIdentity(input, CancellationToken.None);

        // Assert
        result.Result.Should().NotBeNull();
        result.Result.Should().Be(expectedLoginResult);
        await _usersQueryRepository.Received(1).GetUserByCodeAsync(code, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetUserIdentity_WithInvalidCode_ReturnsError()
    {
        // Arrange
        var code = "invalid-code";
        var input = new Code(code).AsMaybe();

        _usersQueryRepository.GetUserByCodeAsync(code, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyUserEntity?)null);

        // Act
        var result = await _sut.GetUserIdentity(input, CancellationToken.None);

        // Assert
        result.Result.Identity.Should().BeNull();
        result.Result.AuthenticationProperties.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Value.Type.Should().Be(ErrorType.IncomingDataIssue);
        result.Error!.Value.ToString().Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task GetUserIdentity_WithValidBasicAuth_ReturnsLoginResultData()
    {
        // Arrange
        var email = "mr.bean@atkinson.com";
        var password = "password123";
        var storedHash = "hashedPassword";
        var input = new Maybe<BasicAuthUserData>(new BasicAuthUserData(email, password), null);
        var user = Substitute.For<IReadOnlyUserEntity>();
        var expectedLoginResult = new LoginResultData(null!, null!);

        user.PasswordHash.Returns(storedHash);
        _usersQueryRepository.GetUserByMailAsync(email, Arg.Any<CancellationToken>())
            .Returns(user);
        _hashHelper.DoesHashEqual(storedHash, password)
            .Returns(true);
        _extractor.GetIdentity(user)
            .Returns(new Maybe<LoginResultData>(expectedLoginResult, null));

        // Act
        var result = await _sut.GetUserIdentity(input, CancellationToken.None);

        // Assert
        result.Result.Should().NotBeNull();
        result.Result.Should().Be(expectedLoginResult);
        await _usersQueryRepository.Received(1).GetUserByMailAsync(email, Arg.Any<CancellationToken>());
    }

    [Fact(Skip = "TODO")]
    public async Task GetUserIdentity_WithInvalidPassword_ReturnsError()
    {
        // Arrange
        var email = "mr.bean@atkinson.com";
        var password = "wrongPassword";
        var storedHash = "correctHash";
        var input = new Maybe<BasicAuthUserData>(new BasicAuthUserData(email, password), null);
        var user = Substitute.For<IReadOnlyUserEntity>();

        user.PasswordHash.Returns(storedHash);
        _usersQueryRepository.GetUserByMailAsync(email, Arg.Any<CancellationToken>())
            .Returns(user);
        _hashHelper.DoesHashEqual(storedHash, password)
            .Returns(false);

        // Act
        var result = await _sut.GetUserIdentity(input, CancellationToken.None);

        // Assert
        result.Result.Identity.Should().BeNull();
        result.Result.AuthenticationProperties.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Value.Type.Should().Be(ErrorType.IncomingDataIssue);
        result.Error!.Value.ToString().Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task GetAllUsers_ReturnsAllUsers()
    {
        // Arrange
        var input = VoidResult.Void;
        var users = new[]
        {
            CreateUserEntity("1", "User1", true, "hash1", 1),
            CreateUserEntity("2", "User2", false, "", 2)
        };

        _usersQueryRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(users);

        // Act
        var result = await _sut.GetAllUsers(input, CancellationToken.None);

        // Assert
        result.Result.Should().NotBeNull();
        var domainUsers = result.Result!.ToList();
        domainUsers.Should().HaveCount(2);
        
        domainUsers[0].Id.Should().Be("1");
        domainUsers[0].Name.Should().Be("User1");
        domainUsers[0].IsAdmin.Should().BeTrue();
        domainUsers[0].HasBasicAuthSetup.Should().BeTrue();
        domainUsers[0].Tenant.Id.Should().Be(1);

        domainUsers[1].Id.Should().Be("2");
        domainUsers[1].Name.Should().Be("User2");
        domainUsers[1].IsAdmin.Should().BeFalse();
        domainUsers[1].HasBasicAuthSetup.Should().BeFalse();
        domainUsers[1].Tenant.Id.Should().Be(2);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsDetailedUserDTO()
    {
        // Arrange
        var userId = "user-2137";
        var input = new Maybe<string>(userId, null);
        var user = CreateUserEntity(userId, "Mr Bean", true, "hash", 42, "code-42");

        _usersQueryRepository.GetUserByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        var result = await _sut.GetByIdAsync(input, CancellationToken.None);

        // Assert
        result.Result.Should().NotBeNull();
        result.Result.Should().NotBeNull();
        result.Result!.Username.Should().Be("Mr Bean");
        result.Result.IsAdmin.Should().BeTrue();
        result.Result.HasRegisteredBasicAuth.Should().BeTrue();
        result.Result.TenantId.Should().Be(42);
        result.Result.Code.Should().Be("code-42");
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsError()
    {
        // Arrange
        var userId = "invalid-id";
        var input = new Maybe<string>(userId, null);

        _usersQueryRepository.GetUserByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyUserEntity?)null);

        // Act
        var result = await _sut.GetByIdAsync(input, CancellationToken.None);

        // Assert
        result.Result.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Value.Type.Should().Be(ErrorType.IncomingDataIssue);
        result.Error!.ToString().Should().Be("User not found!");
    }

    [Fact]
    public async Task AddUserAsync_WithValidUser_AddsUserSuccessfully()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var domainUser = new DomainUser("user-2137", "Mr Bean", false, false, new TenantId(42), now, Gender.Unspecified);
        var input = new Maybe<DomainUser>(domainUser, null);

        _guidProvider.NewGuid().Returns(guid);

        // Act
        var result = await _sut.AddUserAsync(input, CancellationToken.None);

        // Assert
        result.Result.Should().NotBeNull();
        result.Error.Should().BeNull();

        await _usersCommandRepository
            .Received(1)
            .Insert(
                Arg.Is<IReadOnlyUserEntity>(u =>
                    u.id == domainUser.Id &&
                    u.DisplayName == domainUser.Name &&
                    u.IsAdmin == domainUser.IsAdmin &&
                    u.TenantId == domainUser.Tenant.Id &&
                    u.Created == domainUser.Created &&
                    u.Code == guid.ToString()),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddUserAsync_WithNullUser_ReturnsError()
    {
        // Arrange
        var input = new Maybe<DomainUser>(default, null);

        // Act
        var result = await _sut.AddUserAsync(input, CancellationToken.None);

        // Assert
        result.Result.Should().NotBeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Value.Type.Should().Be(ErrorType.IncomingDataIssue);
        result.Error!.ToString().Should().Be("Domain user is null");

        await _usersCommandRepository
            .DidNotReceive()
            .Insert(Arg.Any<IReadOnlyUserEntity>(), Arg.Any<CancellationToken>());
    }

    private static IReadOnlyUserEntity CreateUserEntity(
        string id,
        string name,
        bool isAdmin,
        string passwordHash,
        int tenantId,
        string? code = null)
    {
        var entity = Substitute.For<IReadOnlyUserEntity>();
        entity.id.Returns(id);
        entity.DisplayName.Returns(name);
        entity.IsAdmin.Returns(isAdmin);
        entity.PasswordHash.Returns(passwordHash);
        entity.TenantId.Returns(tenantId);
        entity.Created.Returns(DateTime.UtcNow);
        entity.Code.Returns(code ?? "code");
        entity.Gender.Returns(Gender.Unspecified.ToString());
        return entity;
    }
}
