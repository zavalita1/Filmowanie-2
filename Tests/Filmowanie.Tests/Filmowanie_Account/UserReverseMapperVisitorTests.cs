using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Abstractions;
using Filmowanie.Account.DTOs.Incoming;
using Filmowanie.Account.Visitors;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Filmowanie.Tests.Filmowanie_Account;

public sealed class UserReverseMapperVisitorTests
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IGuidProvider _guidProvider;
    private readonly UserReverseMapperVisitor _visitor;

    public UserReverseMapperVisitorTests()
    {
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _guidProvider = Substitute.For<IGuidProvider>();
        var log = Substitute.For<ILogger<UserReverseMapperVisitor>>();
        _visitor = new UserReverseMapperVisitor(_dateTimeProvider, _guidProvider, log);
    }

    [Fact]
    public void Visit_ValidInput_ReturnsDomainUser()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var guid = Guid.NewGuid();
        _dateTimeProvider.Now.Returns(now);
        _guidProvider.NewGuid().Returns(guid);

        var incomingUserDto = new UserDTO("incomingUserId", "whatever");
        var currentUser = new DomainUser("currentUserId", "currentUserName", true, true, new TenantId(1), DateTime.UtcNow);
        var input = new OperationResult<(UserDTO, DomainUser)>((incomingUserDto, currentUser), null);

        // Act
        var result = _visitor.Visit(input);

        // Assert
        result.Result.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Result.Should().NotBeNull();
        result.Result.Id.Should().Be($"user-{guid}");
        result.Result.Name.Should().Be("incomingUserId");
        result.Result.IsAdmin.Should().BeFalse();
        result.Result.HasBasicAuthSetup.Should().BeFalse();
        result.Result.Tenant.Should().Be(currentUser.Tenant);
        result.Result.Created.Should().Be(now);
    }
}