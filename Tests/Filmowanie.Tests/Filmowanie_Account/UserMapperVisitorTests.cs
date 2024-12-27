using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Abstractions;
using Filmowanie.Account.Visitors;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Filmowanie.Tests.Filmowanie_Account;

public class UserMapperVisitorTests
{
    private readonly ILogger<UserMapperVisitor> _log;
    private readonly UserMapperVisitor _visitor;

    public UserMapperVisitorTests()
    {
        _log = Substitute.For<ILogger<UserMapperVisitor>>();
        _visitor = new UserMapperVisitor(_log);
    }

    [Fact]
    public void Visit_ValidDomainUser_ReturnsUserDTO()
    {
        // Arrange
        var domainUser = new DomainUser("userId", "John Doe", true, true, new TenantId(1), DateTime.UtcNow);
        var operationResult = new OperationResult<DomainUser>(domainUser, null);

        // Act
        var result = _visitor.Visit(operationResult);

        // Assert
        result.Result.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Result.Should().NotBeNull();
        result.Result!.IsAdmin.Should().BeTrue();
        result.Result!.HasRegisteredBasicAuth.Should().BeTrue();
    }
}