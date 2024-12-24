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
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Filmowanie.Tests.Filmowanie_Account;

public sealed class AccountBasicAuthLoginVisitorTests
{
    private readonly IHashHelper _hashHelper;
    private readonly ILoginResultDataExtractor _extractor;
    private readonly AccountBasicAuthLoginVisitor _visitor;
    private UsersRepositoryForTests _usersQueryRepository;


    public AccountBasicAuthLoginVisitorTests()
    {
        _usersQueryRepository = new UsersRepositoryForTests();
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
        var basicAuth = new BasicAuth { Email = "test2@example.com", Password = "password" };
        var data = new OperationResult<BasicAuth>(basicAuth);
        _hashHelper.DoesHashEqual("looo", basicAuth.Password).Returns(false);

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
        _hashHelper.DoesHashEqual("looo", basicAuth.Password).Returns(true);
        var loginResultData = new LoginResultData(null!, null!);
        _extractor.GetIdentity(_usersQueryRepository.MockUsers[1]).Returns(new OperationResult<LoginResultData>(loginResultData));

        // Act
        var result = await _visitor.VisitAsync(data, CancellationToken.None);

        // Assert
        result.Result.Should().Be(loginResultData);
        result.Error.Should().BeNull();
    }

    private sealed class UsersRepositoryForTests : IUsersQueryRepository
    {
        public readonly UserEntity[] MockUsers;

        public UsersRepositoryForTests()
        {
            MockUsers =
            [
                new UserEntity { Email = "loo@to.com" },
                new UserEntity { Email = "test2@example.com", PasswordHash = "looo",  }
            ];
        }

        public Task<IReadOnlyUserEntity?> GetUserAsync(Expression<Func<IReadOnlyUserEntity, bool>> predicate, CancellationToken cancellationToken)
        {
            var result = MockUsers.SingleOrDefault(predicate.Compile());
            return Task.FromResult(result);
        }

        public Task<IReadOnlyUserEntity[]> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(MockUsers.Cast<IReadOnlyUserEntity>().ToArray());
        }
    }
}