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
using System.Linq.Expressions;

namespace Filmowanie.Tests.Filmowanie_Account;

public class AccountCodeLoginVisitorTests
{
    private readonly UsersRepositoryForTests _usersQueryRepository;
    private readonly ILoginResultDataExtractor _extractor;
    private readonly AccountCodeLoginVisitor _visitor;


    public AccountCodeLoginVisitorTests()
    {
        _usersQueryRepository = new UsersRepositoryForTests();
        var log = Substitute.For<ILogger<AccountSignUpVisitor>>();
        _extractor = Substitute.For<ILoginResultDataExtractor>();
        _visitor = new AccountCodeLoginVisitor(_usersQueryRepository, log, _extractor);
    }

    [Fact]
    public async Task VisitAsync_ShouldReturnError_WhenUserIsNull()
    {
        // Arrange
        var result = new OperationResult<string>("testCode");

        // Act
        var operationResult = await _visitor.VisitAsync(result, CancellationToken.None);

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
        var result = await _visitor.VisitAsync(input, CancellationToken.None);

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