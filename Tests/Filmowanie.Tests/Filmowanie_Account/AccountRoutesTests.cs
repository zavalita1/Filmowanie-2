using System.Security.Claims;
using AutoFixture;
using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.DTOs.Incoming;
using Filmowanie.Account.Interfaces;
using Filmowanie.Account.Results;
using Filmowanie.Account.Routes;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Filmowanie.Tests.Filmowanie_Account;

public sealed class AccountRoutesTests
{
    private readonly ILogger<AccountRoutes> _log;
    private readonly IFluentValidatorAdapterProvider _validatorAdapterProvider;
    private readonly ICodeLoginVisitor _accountVisitor;
    private readonly IBasicAuthLoginVisitor _basicAuthLoginVisitor;
    private readonly ISignUpVisitor _signUpVisitor;
    private readonly IUserIdentityVisitor _userIdentityVisitor;
    private readonly IUserMapperVisitor _userMapperVisitor;
    private readonly IHttpContextWrapper _httpContextAccessor;
    private readonly IRoutesResultHelper _routesResultHelper;
    private readonly AccountRoutes _routes;

    private readonly IFixture _fixture = new Fixture();

    public AccountRoutesTests()
    {
        _log = Substitute.For<ILogger<AccountRoutes>>();
        _accountVisitor = Substitute.For<ICodeLoginVisitor>();
        _basicAuthLoginVisitor = Substitute.For<IBasicAuthLoginVisitor>();
        _signUpVisitor = Substitute.For<ISignUpVisitor>();
        _userMapperVisitor = Substitute.For<IUserMapperVisitor>();
        _httpContextAccessor = Substitute.For<IHttpContextWrapper>();
        _userIdentityVisitor = Substitute.For<IUserIdentityVisitor>();
        _validatorAdapterProvider = Substitute.For < IFluentValidatorAdapterProvider>();
        _routesResultHelper = Substitute.For<IRoutesResultHelper>();
        _routes = new AccountRoutes(_log, _validatorAdapterProvider, _accountVisitor, _basicAuthLoginVisitor, _signUpVisitor, _userIdentityVisitor, _userMapperVisitor,
            _routesResultHelper, _httpContextAccessor);
    }

    [Fact]
    public async Task Login_ShouldReturnExpectedResult()
    {
        // Arrange
        var input = _fixture.Create<LoginDto>();
        var code = _fixture.Create<string>();
        var cancellationToken = CancellationToken.None;

        var validator = Substitute.For<IFluentValidatorAdapter<LoginDto>>();
        var operationResult1 = new OperationResult<LoginDto>(new LoginDto(code));
        validator.Validate(input).Returns(operationResult1);
        _validatorAdapterProvider.GetAdapter<LoginDto>().Returns(validator);

        var identity = new ClaimsIdentity();
        var authProps = _fixture.Create<AuthenticationProperties>();
        var operationResult2 = new OperationResult<LoginResultData>(new LoginResultData(identity, authProps));
        _accountVisitor.VisitAsync(Arg.Is<OperationResult<string>>(x => x.Result == code), cancellationToken).Returns(operationResult2);

        var operationResult3 = new OperationResult<DomainUser>(default);
        _userIdentityVisitor.Visit(operationResult2).Returns(operationResult3);

        var operationResult4 = new OperationResult<Account.DTOs.Outgoing.UserDTO>();
        _userMapperVisitor.Visit(operationResult3).Returns(operationResult4);

        var expectedResult = Substitute.For<IResult>();
        _routesResultHelper.UnwrapOperationResult(operationResult4).Returns(expectedResult);

        // Act
        var result = await _routes.Login(input, cancellationToken);

        // Assert
        result.Should().Be(expectedResult);
        await _httpContextAccessor.Received(1).SignInAsync("cookie", Arg.Is<ClaimsPrincipal>(x => x.Identity == identity), operationResult2.Result!.AuthenticationProperties);
    }

    [Fact]
    public async Task LoginBasic_ShouldReturnExpectedResult()
    {
        // Arrange
        var input = _fixture.Create<BasicAuthLoginDTO>();
        var basicAuth = _fixture.Create<BasicAuth>();
        var cancellationToken = CancellationToken.None;

        var validator = Substitute.For<IFluentValidatorAdapter<BasicAuthLoginDTO>>();
        var operationResult1 = new OperationResult<BasicAuthLoginDTO>(new BasicAuthLoginDTO(basicAuth.Email, basicAuth.Password));
        validator.Validate(input).Returns(operationResult1);
        _validatorAdapterProvider.GetAdapter<BasicAuthLoginDTO>("login").Returns(validator);

        var identity = new ClaimsIdentity();
        var authProps = _fixture.Create<AuthenticationProperties>();
        var operationResult2 = new LoginResultData(identity, authProps).ToOperationResult();
        _basicAuthLoginVisitor.VisitAsync(Arg.Is<OperationResult<BasicAuth>>(x => x.Result.Email == basicAuth.Email && x.Result.Password == basicAuth.Password), cancellationToken).Returns(operationResult2);

        var operationResult3 = new OperationResult<DomainUser>(default);
        _userIdentityVisitor.Visit(operationResult2).Returns(operationResult3);

        var operationResult4 = new OperationResult<Account.DTOs.Outgoing.UserDTO>();
        _userMapperVisitor.Visit(operationResult3).Returns(operationResult4);

        var expectedResult = Substitute.For<IResult>();
        _routesResultHelper.UnwrapOperationResult(operationResult4).Returns(expectedResult);

        // Act
        var result = await _routes.LoginBasic(input, cancellationToken);

        // Assert
        await _httpContextAccessor.Received(1).SignInAsync("cookie", Arg.Is<ClaimsPrincipal>(x => x.Identity == identity), operationResult2.Result!.AuthenticationProperties);
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task SignUp_ShouldReturnExpectedResult()
    {
        // Arrange
        var input = _fixture.Create<BasicAuthLoginDTO>();
        var basicAuth = _fixture.Create<BasicAuth>();
        var cancellationToken = CancellationToken.None;

        var validator = Substitute.For<IFluentValidatorAdapter<BasicAuthLoginDTO>>();
        var operationResult1 = new OperationResult<BasicAuthLoginDTO>(new BasicAuthLoginDTO(basicAuth.Email, basicAuth.Password));
        validator.Validate(input).Returns(operationResult1);
        _validatorAdapterProvider.GetAdapter<BasicAuthLoginDTO>("signup").Returns(validator);

        var operationResult2 = new OperationResult<DomainUser>(default!);
        _userIdentityVisitor.Visit(Arg.Is<OperationResult<BasicAuth>>(x => x.Result.Email == basicAuth.Email && x.Result.Password == basicAuth.Password)).Returns(operationResult2);

        var operationResult3 = new OperationResult<LoginResultData>();
        _signUpVisitor.VisitAsync(default, cancellationToken).ReturnsForAnyArgs(operationResult3);

        var operationResult4 = new OperationResult<DomainUser>(default!);
        _userIdentityVisitor.Visit(operationResult3).Returns(operationResult4);

        var operationResult5 = new OperationResult<Account.DTOs.Outgoing.UserDTO>(default!);
        _userMapperVisitor.Visit(operationResult4).Returns(operationResult5);

        var expectedResult = Substitute.For<IResult>();
        _routesResultHelper.UnwrapOperationResult(operationResult5).Returns(expectedResult);

        // Act
        var result = await _routes.SignUp(input, cancellationToken);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task Logout_ShouldReturnExpectedResult()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _routes.Logout(cancellationToken);

        // Assert
        await _httpContextAccessor.Received(1).SignOutAsync("cookie");
        ((Ok)result).StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Get_ShouldReturnExpectedResult()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var operationResult1 = new OperationResult<DomainUser>(default!);
        _userIdentityVisitor.Visit(OperationResultExtensions.Empty).Returns(operationResult1);

        var operationResult2 = new OperationResult<Account.DTOs.Outgoing.UserDTO>(default!);
        _userMapperVisitor.Visit(operationResult1).Returns(operationResult2);

        var expectedResult = Substitute.For<IResult>();
        _routesResultHelper.UnwrapOperationResult(operationResult2).Returns(expectedResult);

        // Act
        var result = await _routes.Get(cancellationToken);

        // Assert
        result.Should().Be(expectedResult);
    }
}