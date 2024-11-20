using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Filmowanie.Abstractions;
using Filmowanie.Account.Constants;
using Filmowanie.DTOs.Incoming;
using Filmowanie.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Account.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = SchemesNamesConsts.Cookie)]
public sealed class AccountController : ControllerBase
{
    private readonly ILogger<AccountController> _log;
    private readonly IFluentValidatorAdapterFactory _validatorAdapterFactory;
    private readonly IAccountService _accountService;
    private readonly IUserIdentityService _userIdentityService;
    private readonly IUserMapper _userMapper;

    public AccountController(ILogger<AccountController> log, IFluentValidatorAdapterFactory validatorAdapterFactory, IAccountService accountService, IUserIdentityService userIdentityService, IUserMapper userMapper)
    {
        _log = log;
        _validatorAdapterFactory = validatorAdapterFactory;
        _accountService = accountService;
        _userIdentityService = userIdentityService;
        _userMapper = userMapper;
    }

    [HttpPost("login/code")]
    [AllowAnonymous]
    [Produces<DTOs.Outgoing.UserDTO>]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancel)
    {
        var validator = _validatorAdapterFactory.GetAdapter<LoginDto>();
        var validationResult = validator.Validate(dto);

        var codeResult = validationResult.Pluck(x => x.Code);
        var result = await _accountService.LoginAsync(codeResult, cancel);

        await result.InvokeAsync(async (r, _) =>
        {
            _log.LogInformation("Logging in...");
            await HttpContext.SignInAsync(SchemesNamesConsts.Cookie, new ClaimsPrincipal(r.Identity), result.Result.AuthenticationProperties);
            _log.LogInformation("Logged in!");
        }, cancel);

        var loggedInUser = _userIdentityService.GetCurrentUser(result);
        var resultDto = _userMapper.Map(loggedInUser);

        return UnwrapOperationResult(resultDto);
    }

    [HttpPost("login/basic")]
    [AllowAnonymous]
    [Produces<DTOs.Outgoing.UserDTO>]
    public async Task<IActionResult> LoginBasic([FromBody] BasicAuthLoginDto dto, CancellationToken cancel)
    {
        var validator = _validatorAdapterFactory.GetAdapter<BasicAuthLoginDto>(KeyedServices.LoginViaBasicAuthKey);
        var validationResult = validator.Validate(dto);
        var basicAuthResult = validationResult.Pluck(x => new BasicAuth(x.Email, x.Password));

        var result = await _accountService.LoginAsync(basicAuthResult, cancel);

        await result.InvokeAsync(async (r, _) =>
        {
            _log.LogInformation("Logging in...");
            await HttpContext.SignInAsync(SchemesNamesConsts.Cookie, new ClaimsPrincipal(r.Identity), result.Result.AuthenticationProperties);
            _log.LogInformation("Logged in!");
        }, cancel);

        var loggedInUser = _userIdentityService.GetCurrentUser(result);
        var resultDto = _userMapper.Map(loggedInUser);

        return UnwrapOperationResult(resultDto);
    }


    [HttpPost("signup")]
    [Produces<DTOs.Outgoing.UserDTO>]
    public async Task<IActionResult> SignUp([FromBody] BasicAuthLoginDto dto, CancellationToken cancel)
    {
        var validator = _validatorAdapterFactory.GetAdapter<BasicAuthLoginDto>(KeyedServices.SignUpBasicAuth);
        var validationResult = validator.Validate(dto);
        var basicAuthResult = validationResult.Pluck(x => new BasicAuth(x.Email, x.Password));
        var loggedInUser = _userIdentityService.GetCurrentUser(basicAuthResult);
        var mergedResult = basicAuthResult.Merge(loggedInUser);

        var result = await _accountService.SignUpAsync(mergedResult, cancel);
        var newMergedResult = result.Merge(loggedInUser);
        loggedInUser = newMergedResult.Pluck(x => x.Item2);
        var resultDto = _userMapper.Map(loggedInUser);

        return UnwrapOperationResult(resultDto);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancel)
    {
        cancel.ThrowIfCancellationRequested();
        await HttpContext.SignOutAsync(SchemesNamesConsts.Cookie);
        return Ok();
    }

    [HttpGet]
    [AllowAnonymous]
    [Produces<DTOs.Outgoing.UserDTO>]
    public Task<IActionResult> Get(CancellationToken cancel)
    {
        var loggedInUser = _userIdentityService.GetCurrentUser(OperationHelper.Empty);
        var resultDto = _userMapper.Map(loggedInUser);

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

