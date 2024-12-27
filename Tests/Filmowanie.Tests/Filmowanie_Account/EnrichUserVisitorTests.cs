using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.Visitors;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Linq.Expressions;

namespace Filmowanie.Tests.Filmowanie_Account;

public sealed class EnrichUserVisitorTests
{
    private readonly EnrichUserVisitor _visitor;

    public EnrichUserVisitorTests()
    {
        var usersRepository = new UsersRepositoryForTests();
        ILogger<EnrichUserVisitor> log = Substitute.For<ILogger<EnrichUserVisitor>>();
        _visitor = new EnrichUserVisitor(usersRepository, log);
    }

    [Fact]
    public async Task VisitAsync_UserNotFound_ReturnsError()
    {
        // Arrange
        var input = new OperationResult<string>("nonexistentUserId", null);

        // Act
        var result = await _visitor.VisitAsync(input, CancellationToken.None);

        // Assert
        result.Result.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Value.ErrorMessages.Should().BeEquivalentTo("User not found!");
        result.Error!.Value.Type.Should().Be(ErrorType.IncomingDataIssue);
    }

    [Fact]
    public async Task VisitAsync_UserFound_ReturnsDetailedUserDTO()
    {
        // Arrange
        var input = new OperationResult<string>("existingUserId", null);

        // Act
        var result = await _visitor.VisitAsync(input, CancellationToken.None);

        // Assert
        result.Result.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Result.Should().NotBeNull();
        result.Result!.IsAdmin.Should().BeTrue();
        result.Result.TenantId.Should().Be(1);
        result.Result.Code.Should().Be("userCode");
    }

    private sealed class UsersRepositoryForTests : IUsersQueryRepository
    {
        private readonly UserEntity[] _mockUsers =
        [
            new() { id = "loo@to.com" },
            new() {   id = "existingUserId",
                DisplayName = "John Doe",
                IsAdmin = true,
                PasswordHash = "hashedPassword",
                TenantId = 1,
                Code = "userCode"
            }
        ];

        public Task<IReadOnlyUserEntity?> GetUserAsync(Expression<Func<IReadOnlyUserEntity, bool>> predicate, CancellationToken cancellationToken)
        {
            var result = _mockUsers.SingleOrDefault(predicate.Compile());
            return Task.FromResult(result);
        }

        public Task<IReadOnlyUserEntity[]> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_mockUsers.Cast<IReadOnlyUserEntity>().ToArray());
        }
    }
}