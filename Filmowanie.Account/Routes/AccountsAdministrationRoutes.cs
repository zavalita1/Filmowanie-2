using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.Constants;
using Filmowanie.Account.DTOs.Incoming;
using Filmowanie.Account.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Account.Routes;

internal class AccountsAdministrationRoutes : IAccountsAdministrationRoutes
{
    private readonly IAccountUserService _userService;
    private readonly IAuthenticationManager _authenticationManager;
    private readonly IUserMapper _mapper;

    private readonly IFluentValidatorAdapter<UserDTO> _validator;
    private readonly IFluentValidatorAdapter<string> _userIdValidator;
    private readonly IRoutesResultHelper _routesResultHelper;

    public AccountsAdministrationRoutes(IFluentValidatorAdapterProvider validatorAdapterProvider, IRoutesResultHelper routesResultHelper, IAccountUserService userService, IAuthenticationManager authenticationManager, IUserMapper mapper)
    {
        _routesResultHelper = routesResultHelper;
        _userService = userService;
        _authenticationManager = authenticationManager;
        _mapper = mapper;
        _validator = validatorAdapterProvider.GetAdapter<UserDTO>(KeyedServices.Username);
        _userIdValidator = validatorAdapterProvider.GetAdapter<string>(KeyedServices.Username);
    }

    public async Task<IResult> GetUsersAsync(CancellationToken cancel)
    {
        var maybeUsers = await _userService.GetAllUsers(VoidResult.Void, cancel);
        return _routesResultHelper.UnwrapOperationResult(maybeUsers);
    }

    public async Task<IResult> GetUserAsync(string userId, CancellationToken cancel)
    {
        var maybeUserId = _userIdValidator.Validate(userId);
        var result = await _userService.GetByIdAsync(maybeUserId, cancel);

        return _routesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> AddUserAsync(UserDTO dto, CancellationToken cancel)
    {
        var maybeDto = _validator.Validate(dto);
        var maybeCurrentUser = _authenticationManager.GetDomainUser(maybeDto.AsVoid());
        var merged = maybeDto.Merge(maybeCurrentUser);
        var maybeUser = _mapper.Map(merged);
        var result = await _userService.AddUserAsync(maybeUser, cancel);

        return _routesResultHelper.UnwrapOperationResult(result, TypedResults.Created());
    }
}