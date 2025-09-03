using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Linq.Expressions;

namespace Filmowanie.Tests.Filmowanie_Account;

public sealed class GetAllUsersVisitorTests
{
    private readonly UsersRepositoryForTests _usersQueryRepository;
    private readonly ILogger<GetAllUsersVisitor> _log;
    private readonly GetAllUsersVisitor _visitor;

    public GetAllUsersVisitorTests()
    {
        _usersQueryRepository = new UsersRepositoryForTests();
        _log = Substitute.For<ILogger<GetAllUsersVisitor>>();
        _visitor = new GetAllUsersVisitor(_usersQueryRepository, _log);
    }

    [Fact]
    public async Task VisitAsync_ValidInput_ReturnsAllUsers()
    {
        // Arrange
        _usersQueryRepository.MockUsers =
        [
            new UserEntity { id = "1", DisplayName = "User One", IsAdmin = true, PasswordHash = "hash1", TenantId = 1, Created = DateTime.UtcNow },
            new UserEntity { id = "2", DisplayName = "User Two", IsAdmin = false, PasswordHash = "hash2", TenantId = 2, Created = DateTime.UtcNow }
        ];

        var input = new OperationResult<string>("input", null);

        // Act
        var result = await _visitor.VisitAsync(input, CancellationToken.None);

        // Assert
        result.Result.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Result.Should().NotBeNull();
        result.Result.Should().HaveCount(2);

        var userList = result.Result.ToList();
        userList[0].Id.Should().Be("1");
        userList[0].Name.Should().Be("User One");
        userList[0].IsAdmin.Should().BeTrue();
        userList[0].HasBasicAuthSetup.Should().BeTrue();
        userList[0].Tenant.Id.Should().Be(1);

        userList[1].Id.Should().Be("2");
        userList[1].Name.Should().Be("User Two");
        userList[1].IsAdmin.Should().BeFalse();
        userList[1].HasBasicAuthSetup.Should().BeTrue();
        userList[1].Tenant.Id.Should().Be(2);
    }

    [Fact]
    public async Task VisitAsync_EmptyUsersList_ReturnsEmptyResult()
    {
        // Arrange
        _usersQueryRepository.MockUsers = [];

        var input = new OperationResult<string>("input", null);

        // Act
        var result = await _visitor.VisitAsync(input, CancellationToken.None);

        // Assert
        result.Result.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Result.Should().NotBeNull();
        result.Result.Should().BeEmpty();
    }

    private sealed class UsersRepositoryForTests : IUsersQueryRepository
    {
        public UserEntity[] MockUsers { get; set; }

        public Task<IReadOnlyUserEntity?> GetUserAsync(Expression<Func<IReadOnlyUserEntity, bool>> predicate, CancellationToken cancellationToken) => throw new NotImplementedException();

        public Task<IReadOnlyUserEntity[]> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(MockUsers.Cast<IReadOnlyUserEntity>().ToArray());
        }
    }
}