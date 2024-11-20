using System.Security.Claims;
using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Account.Constants;
using Filmowanie.Account.Interfaces;
using Filmowanie.DTOs.Incoming;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Account.Routes;

public sealed class AccountRoutes : IAccountRoutes
{
    private readonly ILogger<AccountRoutes> _log;
    private readonly IFluentValidatorAdapterFactory _validatorAdapterFactory;
    private readonly ICodeLoginVisitor _accountVisitor;
    private readonly IBasicAuthLoginVisitor _basicAuthLoginVisitor;
    private readonly ISignUpVisitor _signUpVisitor;
    private readonly IUserIdentityVisitor _userIdentityVisitor;
    private readonly IUserMapperVisitor _userMapperVisitor;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AccountRoutes(ILogger<AccountRoutes> log, IFluentValidatorAdapterFactory validatorAdapterFactory, ICodeLoginVisitor accountVisitor, IBasicAuthLoginVisitor basicAuthLoginVisitor, ISignUpVisitor signUpVisitor, IUserIdentityVisitor userIdentityVisitor, IUserMapperVisitor userMapperVisitor, IHttpContextAccessor httpContextAccessor)
    {
        _log = log;
        _validatorAdapterFactory = validatorAdapterFactory;
        _accountVisitor = accountVisitor;
        _basicAuthLoginVisitor = basicAuthLoginVisitor;
        _signUpVisitor = signUpVisitor;
        _userIdentityVisitor = userIdentityVisitor;
        _userMapperVisitor = userMapperVisitor;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IResult> Login([FromBody] LoginDto dto, CancellationToken cancel)
    {
        var validator = _validatorAdapterFactory.GetAdapter<LoginDto>();

        var result =  await validator.Validate(dto)
            .Pluck(x => x.Code)
            .AcceptAsync(_accountVisitor, cancel);

        await result.AcceptAsync(async (r, _) =>
        {
            _log.LogInformation("Logging in...");
            await _httpContextAccessor.HttpContext!.SignInAsync(Schemes.Cookie, new ClaimsPrincipal(r.Identity), result.Result.AuthenticationProperties);
            _log.LogInformation("Logged in!");
        }, cancel);

        var resultDto = result
            .Accept(_userIdentityVisitor)
            .Accept(_userMapperVisitor);

        return UnwrapOperationResult(resultDto);
    }

    public async Task<IResult> LoginBasic([FromBody] BasicAuthLoginDto dto, CancellationToken cancel)
    {
        var validator = _validatorAdapterFactory.GetAdapter<BasicAuthLoginDto>(KeyedServices.LoginViaBasicAuthKey);
        var result = await validator
            .Validate(dto)
            .Pluck(x => new BasicAuth(x.Email, x.Password))
            .AcceptAsync(_basicAuthLoginVisitor, cancel);

        await result.AcceptAsync(async (r, _) =>
        {
            _log.LogInformation("Logging in...");
            await _httpContextAccessor.HttpContext!.SignInAsync(Schemes.Cookie, new ClaimsPrincipal(r.Identity), result.Result.AuthenticationProperties);
            _log.LogInformation("Logged in!");
        }, cancel);

        var resultDto = result
            .Accept(_userIdentityVisitor)
            .Accept(_userMapperVisitor);

        return UnwrapOperationResult(resultDto);
    }

    public async Task<IResult> SignUp([FromBody] BasicAuthLoginDto dto, CancellationToken cancel)
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

    public async Task<IResult> Logout(CancellationToken cancel)
    {
        cancel.ThrowIfCancellationRequested();
        await _httpContextAccessor.HttpContext!.SignOutAsync(Schemes.Cookie);
        return TypedResults.Ok();
    }

    public Task<IResult> Get(CancellationToken cancel)
    {
        var resultDto = OperationResultExtensions
            .Empty
            .Accept(_userIdentityVisitor)
            .Accept(_userMapperVisitor);

        return Task.FromResult(UnwrapOperationResult(resultDto));
    }

    private static IResult UnwrapOperationResult<T>(OperationResult<T> result)
    {
        if (result.Error == null)
            return TypedResults.Ok(result.Result);

        const string separator = ", ";

        IResult? unwrapped = result.Error!.Value.Type switch
        {
            ErrorType.IncomingDataIssue => TypedResults.BadRequest(result.Error!.Value.ErrorMessages.Concat(separator)),
            ErrorType.ValidationError => TypedResults.BadRequest(result.Error!.Value.ErrorMessages.Concat(separator)),
            ErrorType.AuthorizationIssue => TypedResults.Unauthorized(),
            ErrorType.Canceled => TypedResults.StatusCode(499),
            _ => null
        };

        if (unwrapped != null)
            return unwrapped;

        throw new InvalidOperationException($"Erroneous result! {result.Error.Value.ErrorMessages.Concat(separator)}.");
    }
}

