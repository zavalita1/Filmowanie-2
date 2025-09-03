using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.Interfaces;
using Filmowanie.Account.Results;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Linq.Expressions;
using Filmowanie.Account.Services;

namespace Filmowanie.Tests.Filmowanie_Account;

public class AccountUserServiceTests
{
    private readonly UsersRepositoryForTests _usersQueryRepository;
    private readonly ILoginResultDataExtractor _extractor;
    private readonly AccountUserService _visitor;


    public AccountUserServiceTests()
    {
        _usersQueryRepository = new UsersRepositoryForTests();
        var log = Substitute.For<ILogger<AccountSignUpService>>();
        _extractor = Substitute.For<ILoginResultDataExtractor>();
        _visitor = new AccountUserService(_usersQueryRepository, log, _extractor);
    }

    [Fact]
    public async Task VisitAsync_ShouldReturnError_WhenUserIsNull()
    {
        // Arrange
        var result = new OperationResult<string>("testCode");

        // Act
        var operationResult = await _visitor.GetAllUsers(result, CancellationToken.None);

        // Assert
        operationResult.Result.Should().BeNull();
        operationResult.Error.Should().NotBeNull();
        operationResult.Error!.Value.ErrorMessages.Should().BeEquivalentTo("Invalid credentials");
        operationResult.Error.Value.Type.Should().Be(ErrorType.IncomingDataIssue);
    }

    [Fact]
    public async Task VisitAsync_ShouldReturnLoginResultData_WhenUserIsFound()
    {
        // Arrange
        var input = new OperationResult<string>("looo");
        var loginResultData = new LoginResultData(null!, null!);
        _extractor.GetIdentity(_usersQueryRepository.MockUsers[1]).Returns(new OperationResult<LoginResultData>(loginResultData));

        // Act
        var result = await _visitor.GetAllUsers(input, CancellationToken.None);

        // Assert
        result.Result.Should().Be(loginResultData);
        result.Error.Should().BeNull();
    }

    private sealed class UsersRepositoryForTests : IUsersQueryRepository
    {
        public readonly UserEntity[] MockUsers =
        [
            new() { Email = "loo@to.com" },
            new() { Email = "test2@example.com", Code = "looo",  }
        ];

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