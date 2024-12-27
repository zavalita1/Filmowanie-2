using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Abstractions;
using Filmowanie.Account.Visitors;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Filmowanie.Tests.Filmowanie_Account;

public class AddUserVisitorTests
{
    private readonly IUsersCommandRepository _usersCommandRepository;
    private readonly IGuidProvider _guidProvider;
    private readonly ILogger<AddUserVisitor> _log;
    private readonly AddUserVisitor _visitor;

    public AddUserVisitorTests()
    {
        _usersCommandRepository = Substitute.For<IUsersCommandRepository>();
        _guidProvider = Substitute.For<IGuidProvider>();
        _log = Substitute.For<ILogger<AddUserVisitor>>();
        _visitor = new AddUserVisitor(_usersCommandRepository, _guidProvider, _log);
    }

    [Fact]
    public async Task VisitAsync_ValidDomainUser_InsertsUser()
    {
        // Arrange
        var domainUser = new DomainUser("userId", "John Doe", true, true, new TenantId(1), DateTime.UtcNow);
        var operationResult = new OperationResult<DomainUser>(domainUser, null);
        var guid = Guid.NewGuid();
        _guidProvider.NewGuid().Returns(guid);

        // Act
        var result = await _visitor.VisitAsync(operationResult, CancellationToken.None);

        // Assert
        await _usersCommandRepository.Received(1).Insert(Arg.Is<IReadOnlyUserEntity>(u =>
            u.id == domainUser.Id &&
            u.DisplayName == domainUser.Name &&
            u.IsAdmin == domainUser.IsAdmin &&
            u.TenantId == domainUser.Tenant.Id &&
            u.Created == domainUser.Created &&
            u.Code == guid.ToString()
        ), Arg.Any<CancellationToken>());

        result.Error.Should().BeNull();
    }

    [Fact]
    public async Task VisitAsync_NullDomainUser_ReturnsError()
    {
        // Arrange
        var operationResult = new OperationResult<DomainUser>(default, new Error("Domain user is null", ErrorType.IncomingDataIssue));

        // Act
        var result = await _visitor.VisitAsync(operationResult, CancellationToken.None);

        // Assert
        await _usersCommandRepository.DidNotReceive().Insert(Arg.Any<IReadOnlyUserEntity>(), Arg.Any<CancellationToken>());

        result.Result.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Value.ErrorMessages.Should().Contain("Domain user is null");
        result.Error.Value.Type.Should().Be(ErrorType.IncomingDataIssue);
    }
}