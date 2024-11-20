using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Account.Constants;
using Filmowanie.Account.Interfaces;
using Filmowanie.DTOs.Incoming;
using Filmowanie.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Account.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = Schemes.Cookie)]
public sealed class AccountController : ControllerBase
{
    private readonly ILogger<AccountController> _log;
    private readonly IFluentValidatorAdapterFactory _validatorAdapterFactory;
    private readonly ICodeLoginVisitor _accountVisitor;
    private readonly IBasicAuthLoginVisitor _basicAuthLoginVisitor;
    private readonly ISignUpVisitor _signUpVisitor;
    private readonly IUserIdentityVisitor _userIdentityVisitor;
    private readonly IUserMapperVisitor _userMapperVisitor;

    public AccountController(ILogger<AccountController> log, IFluentValidatorAdapterFactory validatorAdapterFactory, ICodeLoginVisitor accountVisitor, IBasicAuthLoginVisitor basicAuthLoginVisitor, ISignUpVisitor signUpVisitor, IUserIdentityVisitor userIdentityVisitor, IUserMapperVisitor userMapperVisitor)
    {
        _log = log;
        _validatorAdapterFactory = validatorAdapterFactory;
        _accountVisitor = accountVisitor;
        _basicAuthLoginVisitor = basicAuthLoginVisitor;
        _signUpVisitor = signUpVisitor;
        _userIdentityVisitor = userIdentityVisitor;
        _userMapperVisitor = userMapperVisitor;
    }

    [HttpPost("login/code")]
    [AllowAnonymous]
    [Produces<DTOs.Outgoing.UserDTO>]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancel)
    {
        var validator = _validatorAdapterFactory.GetAdapter<LoginDto>();

        var result =  await validator.Validate(dto)
            .Pluck(x => x.Code)
            .AcceptAsync(_accountVisitor, cancel);

        await result.AcceptAsync(async (r, _) =>
        {
            _log.LogInformation("Logging in...");
            await HttpContext.SignInAsync(Schemes.Cookie, new ClaimsPrincipal(r.Identity), result.Result.AuthenticationProperties);
            _log.LogInformation("Logged in!");
        }, cancel);

        var resultDto = result
            .Accept(_userIdentityVisitor)
            .Accept(_userMapperVisitor);

        return UnwrapOperationResult(resultDto);
    }

    [HttpPost("login/basic")]
    [AllowAnonymous]
    [Produces<DTOs.Outgoing.UserDTO>]
    public async Task<IActionResult> LoginBasic([FromBody] BasicAuthLoginDto dto, CancellationToken cancel)
    {
        var validator = _validatorAdapterFactory.GetAdapter<BasicAuthLoginDto>(KeyedServices.LoginViaBasicAuthKey);
        var result = await validator
            .Validate(dto)
            .Pluck(x => new BasicAuth(x.Email, x.Password))
            .AcceptAsync(_basicAuthLoginVisitor, cancel);

        await result.AcceptAsync(async (r, _) =>
        {
            _log.LogInformation("Logging in...");
            await HttpContext.SignInAsync(Schemes.Cookie, new ClaimsPrincipal(r.Identity), result.Result.AuthenticationProperties);
            _log.LogInformation("Logged in!");
        }, cancel);

        var resultDto = result
            .Accept(_userIdentityVisitor)
            .Accept(_userMapperVisitor);

        return UnwrapOperationResult(resultDto);
    }


    [HttpPost("signup")]
    [Produces<DTOs.Outgoing.UserDTO>]
    public async Task<IActionResult> SignUp([FromBody] BasicAuthLoginDto dto, CancellationToken cancel)
    {
        var validator = _validatorAdapterFactory.GetAdapter<BasicAuthLoginDto>(KeyedServices.SignUpBasicAuth);
        var basicAuthResult = validator.Validate(dto).Pluck(x => new BasicAuth(x.Email, x.Password));
        var resultDto = (await basicAuthResult
                .Accept(_userIdentityVisitor)
                .Merge(basicAuthResult)
                .AcceptAsync(_signUpVisitor, cancel))
            .Accept(_userIdentityVisitor)
            .Accept(_userMapperVisitor);

        return UnwrapOperationResult(resultDto);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancel)
    {
        cancel.ThrowIfCancellationRequested();
        await HttpContext.SignOutAsync(Schemes.Cookie);
        return Ok();
    }

    [HttpGet]
    [AllowAnonymous]
    [Produces<DTOs.Outgoing.UserDTO>]
    public Task<IActionResult> Get(CancellationToken cancel)
    {
        var resultDto = OperationResultExtensions
            .Empty
            .Accept(_userIdentityVisitor)
            .Accept(_userMapperVisitor);

        return Task.FromResult(UnwrapOperationResult(resultDto));
    }

    private IActionResult UnwrapOperationResult<T>(OperationResult<T> result)
    {
        if (result.Error != null)
            return Ok(result.Result);

        const string separator = ", ";

        IActionResult unwrapped = result.Error!.Value.Type switch
        {
            ErrorType.IncomingDataIssue => BadRequest(result.Error!.Value.ErrorMessages.Concat(separator)),
            ErrorType.ValidationError => BadRequest(result.Error!.Value.ErrorMessages.Concat(separator)),
            ErrorType.AuthorizationIssue => Unauthorized(),
            ErrorType.Canceled => StatusCode(499),
            _ => null
        };

        if (unwrapped != null)
            return unwrapped;

        throw new InvalidOperationException($"Erroneous result! {result.Error.Value.ErrorMessages.Concat(separator)}.");
    }
}

