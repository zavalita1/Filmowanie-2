using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.Constants;
using Filmowanie.Account.DTOs.Incoming;
using Filmowanie.Account.Interfaces;
using Filmowanie.Account.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Filmowanie.Account.Routes;

internal sealed class AccountRoutes : IAccountRoutes
{
    private readonly IFluentValidatorAdapterProvider validatorAdapterProvider;
    private readonly IAccountUserService userService;
    private readonly IGoogleAuthService googleAuthService;
    private readonly IAuthenticationManager authenticationManager;
    private readonly IUserDtoMapper userMapper;
    private readonly ISignUpService signUpService;
    private readonly IRoutesResultHelper routesResultHelper;

    public AccountRoutes(IFluentValidatorAdapterProvider validatorAdapterProvider, IAccountUserService userService, IGoogleAuthService googleAuthService, ISignUpService signUpService, IUserDtoMapper userMapper, IRoutesResultHelper routesResultHelper, IAuthenticationManager authenticationManager)
    {
        this.validatorAdapterProvider = validatorAdapterProvider;
        this.userService = userService;
        this.googleAuthService = googleAuthService;
        this.signUpService = signUpService;
        this.userMapper = userMapper;
        this.routesResultHelper = routesResultHelper;
        this.authenticationManager = authenticationManager;
    }

    public async Task<IResult> LoginAsync([FromBody] LoginDto dto, CancellationToken cancel)
    {
        var validator = this.validatorAdapterProvider.GetAdapter<LoginDto>();

        var maybeCode =  validator.Validate(dto).Map(x => new Code(x.Code));
        var maybeIdentity = await this.userService.GetUserIdentity(maybeCode, cancel);
        var voidResult = await this.authenticationManager.LogInAsync(maybeIdentity, cancel);
        var domainUser = this.authenticationManager.GetDomainUser(voidResult);
        var resultDto = userMapper.Map(domainUser);

        return routesResultHelper.UnwrapOperationResult(resultDto);
    }

    public async Task<IResult> LoginBasicAsync([FromBody] BasicAuthLoginDTO dto, CancellationToken cancel)
    {
        var validator = this.validatorAdapterProvider.GetAdapter<BasicAuthLoginDTO>(KeyedServices.LoginViaBasicAuthKey);
        var maybeBasicAuth = validator.Validate(dto).Map(x => new BasicAuthUserData(x.Email, x.Password));
        var maybeIdentity = await this.userService.GetUserIdentity(maybeBasicAuth, cancel);
        var voidResult = await this.authenticationManager.LogInAsync(maybeIdentity, cancel);
        var domainUser = this.authenticationManager.GetDomainUser(voidResult);
        var resultDto = this.userMapper.Map(domainUser);

        return routesResultHelper.UnwrapOperationResult(resultDto);
    }

    public async Task<IResult> LoginGoogleAsync([FromBody] GoogleOAuthClientDTO dto, CancellationToken cancel)
    {
        var validator = this.validatorAdapterProvider.GetAdapter<GoogleOAuthClientDTO>();
        var maybeCode = validator.Validate(dto).Map(x => new GoogleCode(x.Code));
        var maybeUserData = await this.googleAuthService.GetUserData(maybeCode, cancel);
        var maybeIdentity = await this.userService.GetUserIdentity(maybeUserData, cancel);
        var voidResult = await authenticationManager.LogInAsync(maybeIdentity, cancel);
        var domainUser = this.authenticationManager.GetDomainUser(voidResult);
        var resultDto = this.userMapper.Map(domainUser);

        return routesResultHelper.UnwrapOperationResult(resultDto);
    }

    public async Task<IResult> SignUpAsync([FromBody] BasicAuthLoginDTO dto, CancellationToken cancel)
    {
        var validator = this.validatorAdapterProvider.GetAdapter<BasicAuthLoginDTO>(KeyedServices.SignUpBasicAuth);
        var maybeBasicAuth = validator.Validate(dto).Map(x => new BasicAuthUserData(x.Email, x.Password));
        var maybeIdentity = await this.userService.GetUserIdentity(maybeBasicAuth, cancel);
        var maybeDomainUser = this.authenticationManager.GetDomainUser(maybeIdentity);
        var maybeLoginData = await signUpService.SignUp(maybeDomainUser, maybeBasicAuth, cancel);
        maybeDomainUser = this.authenticationManager.GetDomainUser(maybeLoginData);
        var resultDto = this.userMapper.Map(maybeDomainUser);

        return this.routesResultHelper.UnwrapOperationResult(resultDto);
    }

    public async Task<IResult> LogoutAsync(CancellationToken cancel)
    {
        var result = await this.authenticationManager.LogOutAsync(VoidResult.Void, cancel);
        return this.routesResultHelper.UnwrapOperationResult(result);
    }

    public IResult Get(CancellationToken cancel)
    {
        var maybeDomainUser = this.authenticationManager.GetDomainUser(VoidResult.Void);
        var resultDto = this.userMapper.Map(maybeDomainUser);

        return this.routesResultHelper.UnwrapOperationResult(resultDto);
    }
}