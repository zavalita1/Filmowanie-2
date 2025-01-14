using System.Security.Claims;
using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Constants;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Account.Constants;
using Filmowanie.Account.DTOs.Incoming;
using Filmowanie.Account.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Account.Routes;

internal sealed class AccountRoutes : IAccountRoutes
{
    private readonly ILogger<AccountRoutes> _log;
    private readonly IFluentValidatorAdapterProvider _validatorAdapterProvider;
    private readonly ICodeLoginVisitor _accountVisitor;
    private readonly IBasicAuthLoginVisitor _basicAuthLoginVisitor;
    private readonly ISignUpVisitor _signUpVisitor;
    private readonly IUserIdentityVisitor _userIdentityVisitor;
    private readonly IUserMapperVisitor _userMapperVisitor;
    private readonly IRoutesResultHelper _routesResultHelper;
    private readonly IHttpContextWrapper _httpContextWrapper;

    public AccountRoutes(ILogger<AccountRoutes> log, IFluentValidatorAdapterProvider validatorAdapterProvider, ICodeLoginVisitor accountVisitor, IBasicAuthLoginVisitor basicAuthLoginVisitor, ISignUpVisitor signUpVisitor, IUserIdentityVisitor userIdentityVisitor, IUserMapperVisitor userMapperVisitor, IRoutesResultHelper routesResultHelper, IHttpContextWrapper httpContextWrapper)
    {
        _log = log;
        _validatorAdapterProvider = validatorAdapterProvider;
        _accountVisitor = accountVisitor;
        _basicAuthLoginVisitor = basicAuthLoginVisitor;
        _signUpVisitor = signUpVisitor;
        _userIdentityVisitor = userIdentityVisitor;
        _userMapperVisitor = userMapperVisitor;
        _routesResultHelper = routesResultHelper;
        _httpContextWrapper = httpContextWrapper;
    }

    public async Task<IResult> Login([FromBody] LoginDto dto, CancellationToken cancel)
    {
        var validator = _validatorAdapterProvider.GetAdapter<LoginDto>();

        var result =  await validator.Validate(dto)
            .Pluck(x => x.Code)
            .AcceptAsync(_accountVisitor, cancel);

        await result.AcceptAsync(async (r, _) =>
        {
            _log.LogInformation("Logging in...");
            var claimsPrincipal = new ClaimsPrincipal(r.Identity);
            await _httpContextWrapper.SignInAsync(Schemes.Cookie, claimsPrincipal, result.Result!.AuthenticationProperties);
            _log.LogInformation("Logged in!");
        }, cancel);

        var resultDto = result
            .Accept(_userIdentityVisitor)
            .Accept(_userMapperVisitor);

        return _routesResultHelper.UnwrapOperationResult(resultDto);
    }

    public async Task<IResult> LoginBasic([FromBody] BasicAuthLoginDTO dto, CancellationToken cancel)
    {
        var validator = _validatorAdapterProvider.GetAdapter<BasicAuthLoginDTO>(KeyedServices.LoginViaBasicAuthKey);
        var result = await validator
            .Validate(dto)
            .Pluck(x => new BasicAuth(x.Email, x.Password))
            .AcceptAsync(_basicAuthLoginVisitor, cancel);

        await result.AcceptAsync(async (r, _) =>
        {
            _log.LogInformation("Logging in...");
            await _httpContextWrapper.SignInAsync(Schemes.Cookie, new ClaimsPrincipal(r.Identity), r.AuthenticationProperties);
            _log.LogInformation("Logged in!");
        }, cancel);

        var resultDto = result
            .Accept(_userIdentityVisitor)
            .Accept(_userMapperVisitor);

        return _routesResultHelper.UnwrapOperationResult(resultDto);
    }

    public async Task<IResult> SignUp([FromBody] BasicAuthLoginDTO dto, CancellationToken cancel)
    {
        var validator = _validatorAdapterProvider.GetAdapter<BasicAuthLoginDTO>(KeyedServices.SignUpBasicAuth);
        var basicAuthResult = validator.Validate(dto).Pluck(x => new BasicAuth(x.Email, x.Password));
        var resultDto = (await basicAuthResult
                .Accept(_userIdentityVisitor)
                .Merge(basicAuthResult)
                .AcceptAsync(_signUpVisitor, cancel))
            .Accept(_userIdentityVisitor)
            .Accept(_userMapperVisitor);

        return _routesResultHelper.UnwrapOperationResult(resultDto);
    }

    public async Task<IResult> Logout(CancellationToken cancel)
    {
        cancel.ThrowIfCancellationRequested();
        await _httpContextWrapper.SignOutAsync(Schemes.Cookie);
        return TypedResults.Ok();
    }

    public Task<IResult> Get(CancellationToken cancel)
    {
        var resultDto = OperationResultExtensions
            .Empty
            .Accept(_userIdentityVisitor)
            .Accept(_userMapperVisitor);

        return Task.FromResult(_routesResultHelper.UnwrapOperationResult(resultDto));
    }
}