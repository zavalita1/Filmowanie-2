using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Account.Constants;
using Filmowanie.Account.DTOs.Incoming;
using Filmowanie.Account.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Account.Routes;

internal class AccountsAdministrationRoutes : IAccountsAministrationRoutes
{
    private readonly IEnrichUserVisitor _enrichUserVisitor;
    private readonly IGetAllUsersVisitor _getAllUsersVisitor;
    private readonly IUserReverseMapperVisitor _reverseMapperVisitor;
    private readonly IAddUserVisitor _addUserVisitor;
    private readonly IUserIdentityVisitor _userIdentityVisitor;
    private readonly IFluentValidatorAdapter<UserDTO> _validator;
    private readonly IFluentValidatorAdapter<string> _userIdValidator;
    private readonly IRoutesResultHelper _routesResultHelper;

    public AccountsAdministrationRoutes(IFluentValidatorAdapterProvider validatorAdapterProvider, IEnrichUserVisitor enrichUserVisitor, IGetAllUsersVisitor getAllUsersVisitor, IUserReverseMapperVisitor reverseMapperVisitor, IAddUserVisitor addUserVisitor, IUserIdentityVisitor userIdentityVisitor, IRoutesResultHelper routesResultHelper)
    {
        _enrichUserVisitor = enrichUserVisitor;
        _getAllUsersVisitor = getAllUsersVisitor;
        _reverseMapperVisitor = reverseMapperVisitor;
        _addUserVisitor = addUserVisitor;
        _userIdentityVisitor = userIdentityVisitor;
        _routesResultHelper = routesResultHelper;
        _validator = validatorAdapterProvider.GetAdapter<UserDTO>(KeyedServices.Username);
        _userIdValidator = validatorAdapterProvider.GetAdapter<string>(KeyedServices.Username);
    }

    public async Task<IResult> GetUsers(CancellationToken cancel)
    {
        var result = await OperationResultExtensions
            .Empty
            .AcceptAsync(_getAllUsersVisitor, cancel);

        return _routesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> GetUser(string userId, CancellationToken cancel)
    {
        var result = await _userIdValidator
            .Validate(userId)
            .AcceptAsync(_enrichUserVisitor, cancel);

        return _routesResultHelper.UnwrapOperationResult(result);
    }

    public async Task<IResult> AddUser(UserDTO dto, CancellationToken cancel)
    {
        var validationResult = _validator.Validate(dto);
        var currentUser = validationResult.Accept(_userIdentityVisitor);
        var result = await validationResult.Merge(currentUser)
            .Accept(_reverseMapperVisitor)
            .AcceptAsync(_addUserVisitor, cancel);

        return _routesResultHelper.UnwrapOperationResult(result, TypedResults.Created());
    }
}