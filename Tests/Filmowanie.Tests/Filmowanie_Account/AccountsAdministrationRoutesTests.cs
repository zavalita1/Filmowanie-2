using AutoFixture;
using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.DTOs.Outgoing;
using Filmowanie.Account.Interfaces;
using Filmowanie.Account.Routes;
using Filmowanie.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using UserDTO = Filmowanie.Account.DTOs.Incoming.UserDTO;

namespace Filmowanie.Tests.Filmowanie_Account;

public sealed class AccountsAdministrationRoutesTests
{
    private readonly IEnrichUserVisitor _enrichUserVisitor;
    private readonly IGetAllUsersVisitor _getAllUsersVisitor;
    private readonly IUserReverseMapperVisitor _reverseMapperVisitor;
    private readonly IAddUserVisitor _addUserVisitor;
    private readonly IUserIdentityVisitor _userIdentityVisitor;
    private readonly IRoutesResultHelper _routesResultHelper;
    private readonly IFluentValidatorAdapterProvider _validatorProvider;
    private readonly AccountsAdministrationRoutes _routes;

    private readonly IFixture _fixture = new Fixture();

    public AccountsAdministrationRoutesTests()
    {
        _enrichUserVisitor = Substitute.For<IEnrichUserVisitor>();
        _getAllUsersVisitor = Substitute.For<IGetAllUsersVisitor>();
        _reverseMapperVisitor = Substitute.For<IUserReverseMapperVisitor>();
        _addUserVisitor = Substitute.For<IAddUserVisitor>();
        _userIdentityVisitor = Substitute.For<IUserIdentityVisitor>();
        _validatorProvider = Substitute.For < IFluentValidatorAdapterProvider>();
        _routesResultHelper = Substitute.For<IRoutesResultHelper>();
        _routes = new AccountsAdministrationRoutes(_validatorProvider, _enrichUserVisitor, _getAllUsersVisitor, _reverseMapperVisitor, _addUserVisitor, _userIdentityVisitor, _routesResultHelper);
    }

    [Fact]
    public async Task GetUsers_ShouldReturnExpectedResult()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var operationResult1 = new OperationResult<IEnumerable<DomainUser>>(default!);
        _getAllUsersVisitor.VisitAsync(OperationResultExtensions.Empty, cancellationToken).Returns(operationResult1);

        var expectedResult = Substitute.For<IResult>();
        _routesResultHelper.UnwrapOperationResult(operationResult1).Returns(expectedResult);

        // Act
        var result = await _routes.GetUsers(cancellationToken);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task GetUser_ShouldReturnExpectedResult()
    {
        // Arrange
        var input = _fixture.Create<string>();
        var cancellationToken = CancellationToken.None;

        var validator = Substitute.For<IFluentValidatorAdapter<string>>();
        var operationResult1 = new OperationResult<string>(default!);
        validator.Validate(input).Returns(operationResult1);
        _validatorProvider.GetAdapter<string>("username").Returns(validator);

        var operationResult2 = new OperationResult<DetailedUserDTO>(default!);
        _enrichUserVisitor.VisitAsync(operationResult1, cancellationToken).Returns(operationResult2);

        var expectedResult = Substitute.For<IResult>();
        _routesResultHelper.UnwrapOperationResult(operationResult2).Returns(expectedResult);

        // Act
        var result = await _routes.GetUser(input, cancellationToken);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task AddUser_ShouldReturnExpectedResult()
    {
        // Arrange
        var input = _fixture.Create<UserDTO>();
        var cancellationToken = CancellationToken.None;

        var validator = Substitute.For<IFluentValidatorAdapter<UserDTO>>();
        var operationResult1 = new OperationResult<UserDTO>(default!);
        validator.Validate(input).Returns(operationResult1);
        _validatorProvider.GetAdapter<UserDTO>("username").Returns(validator);

        var operationResult2 = new OperationResult<DomainUser>(default!);
        _userIdentityVisitor.Visit(operationResult1).Returns(operationResult2);

        var operationResult3 = new OperationResult<DomainUser>();
        _reverseMapperVisitor.Visit(OperationResultHelpers.GetEquivalent(operationResult1, operationResult2)).Returns(operationResult3);

        var operationResult4 = new OperationResult<object>(default!);
        _addUserVisitor.VisitAsync(operationResult3, cancellationToken).Returns(operationResult4);

        var expectedResult = Substitute.For<IResult>();
        _routesResultHelper.UnwrapOperationResult(operationResult4).Returns(expectedResult);

        // Act
        var result = await _routes.AddUser(input, cancellationToken);

        // Assert
        result.Should().Be(expectedResult);
    }
}