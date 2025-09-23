using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.Constants;
using Filmowanie.Account.DTOs.Incoming;
using Filmowanie.Account.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Account.Routes;

internal class AccountsAdministrationRoutes : IAccountsAdministrationRoutes
{
    private readonly IAccountUserService userService;
    private readonly IAuthenticationManager authenticationManager;
    private readonly IDomainUserMapper mapper;

    private readonly IFluentValidatorAdapter<UserDTO> validator;
    private readonly IFluentValidatorAdapter<string> userIdValidator;
    private readonly IRoutesResultHelper routesResultHelper;

    public AccountsAdministrationRoutes(IFluentValidatorAdapterProvider validatorAdapterProvider, IRoutesResultHelper routesResultHelper, IAccountUserService userService, IAuthenticationManager authenticationManager, IDomainUserMapper mapper)
    {
        this.routesResultHelper = routesResultHelper;
        this.userService = userService;
        this.authenticationManager = authenticationManager;
        this.mapper = mapper;
        validator = validatorAdapterProvider.GetAdapter<UserDTO>(KeyedServices.Username);
        userIdValidator = validatorAdapterProvider.GetAdapter<string>(KeyedServices.Username);
    }

    public async Task<IResult> GetUsersAsync(CancellationToken cancel)
    {
        var maybeUsers = await this.userService.GetAllUsers(VoidResult.Void, cancel);
        return this.routesResultHelper.UnwrapOperationResult(maybeUsers);
    }

    public async Task<IResult> GetUserAsync(string userId, CancellationToken cancel)
    {
        var maybeUserId = userIdValidator.Validate(userId);
        var result = await this.userService.GetByIdAsync(maybeUserId, cancel);

        return this.routesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> AddUserAsync(UserDTO dto, CancellationToken cancel)
    {
        var maybeDto = validator.Validate(dto);
        var maybeCurrentUser = this.authenticationManager.GetDomainUser(maybeDto);
        var maybeUser = mapper.Map(maybeDto, maybeCurrentUser);
        var result = await this.userService.AddUserAsync(maybeUser, cancel);

        return this.routesResultHelper.UnwrapOperationResult(result, TypedResults.Created());
    }
}