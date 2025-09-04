using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.Constants;
using Filmowanie.Account.DTOs.Incoming;
using Filmowanie.Account.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Filmowanie.Account.Routes;

internal sealed class AccountRoutes : IAccountRoutes
{
    private readonly IFluentValidatorAdapterProvider _validatorAdapterProvider;
    private readonly IAccountUserService _userService;
    private readonly IAuthenticationManager _authenticationManager;
    private readonly IUserMapper _userMapper;
    private readonly ISignUpService _signUpService;
    private readonly IRoutesResultHelper _routesResultHelper;

    public AccountRoutes(IFluentValidatorAdapterProvider validatorAdapterProvider, IAccountUserService userService, ISignUpService signUpService, IUserMapper userMapper, IRoutesResultHelper routesResultHelper, IAuthenticationManager authenticationManager)
    {
        _validatorAdapterProvider = validatorAdapterProvider;
        _userService = userService;
        _signUpService = signUpService;
        _userMapper = userMapper;
        _routesResultHelper = routesResultHelper;
        _authenticationManager = authenticationManager;
    }

    public async Task<IResult> LoginAsync([FromBody] LoginDto dto, CancellationToken cancel)
    {
        var validator = _validatorAdapterProvider.GetAdapter<LoginDto>();

        var maybeCode =  validator.Validate(dto).Map(x => x.Code);
        var maybeIdentity = await _userService.GetUserIdentity(maybeCode, cancel);
        var voidResult = await _authenticationManager.LogInAsync(maybeIdentity, cancel);
        var domainUser = _authenticationManager.GetDomainUser(voidResult);
        var resultDto = _userMapper.Map(domainUser);

        return _routesResultHelper.UnwrapOperationResult(resultDto);
    }

    public async Task<IResult> LoginBasicAsync([FromBody] BasicAuthLoginDTO dto, CancellationToken cancel)
    {
        var validator = _validatorAdapterProvider.GetAdapter<BasicAuthLoginDTO>(KeyedServices.LoginViaBasicAuthKey);
        var maybeBasicAuth = validator.Validate(dto).Map(x => new BasicAuth(x.Email, x.Password));
        var maybeIdentity = await _userService.GetUserIdentity(maybeBasicAuth, cancel);
        var voidResult = await _authenticationManager.LogInAsync(maybeIdentity, cancel);
        var domainUser = _authenticationManager.GetDomainUser(voidResult);
        var resultDto = _userMapper.Map(domainUser);

        return _routesResultHelper.UnwrapOperationResult(resultDto);
    }

    public async Task<IResult> SignUpAsync([FromBody] BasicAuthLoginDTO dto, CancellationToken cancel)
    {
        var validator = _validatorAdapterProvider.GetAdapter<BasicAuthLoginDTO>(KeyedServices.SignUpBasicAuth);
        var maybeBasicAuth = validator.Validate(dto).Map(x => new BasicAuth(x.Email, x.Password));
        var maybeIdentity = await _userService.GetUserIdentity(maybeBasicAuth, cancel);
        var maybeDomainUser = _authenticationManager.GetDomainUser(maybeIdentity.AsVoid());
        var merged = maybeDomainUser.Merge(maybeBasicAuth);
        var maybeLoginData = await _signUpService.SignUp(merged, cancel);
        maybeDomainUser = _authenticationManager.GetDomainUser(maybeLoginData.AsVoid());
        var resultDto = _userMapper.Map(maybeDomainUser);

        return _routesResultHelper.UnwrapOperationResult(resultDto);
    }

    public async Task<IResult> LogoutAsync(CancellationToken cancel)
    {
        var result = await _authenticationManager.LogOutAsync(VoidResult.Void, cancel);
        return _routesResultHelper.UnwrapOperationResult(result);
    }

    public IResult Get(CancellationToken cancel)
    {
        var maybeDomainUser = _authenticationManager.GetDomainUser(VoidResult.Void);
        var resultDto = _userMapper.Map(maybeDomainUser);

        return _routesResultHelper.UnwrapOperationResult(resultDto);
    }
}