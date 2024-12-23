using System.Linq.Expressions;
using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.Interfaces;
using Filmowanie.Account.Results;
using Filmowanie.Account.Visitors;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Filmowanie.Tests.Filmowanie_Account;

public class AccountBasicAuthLoginVisitorTests
{
    private readonly IUsersQueryRepository _usersQueryRepository;
    private readonly IHashHelper _hashHelper;
    private readonly ILoginResultDataExtractor _extractor;
    private readonly AccountBasicAuthLoginVisitor _visitor;

    public AccountBasicAuthLoginVisitorTests()
    {
        _usersQueryRepository = Substitute.For<IUsersQueryRepository>();
        _hashHelper = Substitute.For<IHashHelper>();
        var log = Substitute.For<ILogger<AccountSignUpVisitor>>();
        _extractor = Substitute.For<ILoginResultDataExtractor>();
        _visitor = new AccountBasicAuthLoginVisitor(_usersQueryRepository, _hashHelper, log, _extractor);
    }

    [Fact]
    public async Task VisitAsync_ShouldReturnInvalidCredentialsError_WhenUserNotFound()
    {
        // Arrange
        var basicAuth = new BasicAuth { Email = "test@example.com", Password = "password" };
        var data = new OperationResult<BasicAuth>(basicAuth);
        _usersQueryRepository.GetUserAsync()
            .Returns(Task.FromResult<IReadOnlyUserEntity>(null));

        // Act
        var result = await _visitor.VisitAsync(data, CancellationToken.None);

        // Assert
        result.Error.Should().NotBeNull();
        result.Error.Value.ErrorMessages.Should().Contain("Invalid credentials");
        result.Error.Value.Type.Should().Be(ErrorType.IncomingDataIssue);
    }

    [Fact]
    public async Task VisitAsync_ShouldReturnInvalidCredentialsError_WhenPasswordDoesNotMatch()
    {
        // Arrange
        var basicAuth = new BasicAuth { Email = "test@example.com", Password = "password" };
        var data = new OperationResult<BasicAuth>(basicAuth);
        var user = new User { Email = "test@example.com", PasswordHash = "hashedPassword" };
        _usersQueryRepository.GetUserAsync(Arg.Any<Func<User, bool>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(user));
        _hashHelper.DoesHashEqual(user.PasswordHash, basicAuth.Password).Returns(false);

        // Act
        var result = await _visitor.VisitAsync(data, CancellationToken.None);

        // Assert
        result.Error.Should().NotBeNull();
        result.Error.Value.ErrorMessages.Should().Contain("Invalid credentials");
        result.Error.Value.Type.Should().Be(ErrorType.IncomingDataIssue);
    }

    [Fact]
    public async Task VisitAsync_ShouldReturnLoginResultData_WhenCredentialsAreValid()
    {
        // Arrange
        var basicAuth = new BasicAuth { Email = "test@example.com", Password = "password" };
        var data = new OperationResult<BasicAuth>(basicAuth);
        var user = new User { Email = "test@example.com", PasswordHash = "hashedPassword" };
        var loginResultData = new LoginResultData();
        _usersQueryRepository.GetUserAsync(Arg.Any<Func<User, bool>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(user));
        _hashHelper.DoesHashEqual(user.PasswordHash, basicAuth.Password).Returns(true);
        _extractor.GetIdentity(user).Returns(new OperationResult<LoginResultData>(loginResultData));

        // Act
        var result = await _visitor.VisitAsync(data, CancellationToken.None);

        // Assert
        result.Result.Should().Be(loginResultData);
        result.Error.Should().BeNull();
    }
}