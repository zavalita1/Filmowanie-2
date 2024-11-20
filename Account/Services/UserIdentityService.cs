using System.Globalization;
using System.Linq;
using Filmowanie.Abstractions;
using Filmowanie.Account.Constants;
using Filmowanie.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Account.Services;

public sealed class UserIdentityService : IUserIdentityService
{
    private readonly IHttpContextAccessor _contextAccessor;

    public UserIdentityService(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public OperationResult<DomainUser> GetCurrentUser<T>(OperationResult<T> operationResult)
    {
        if (operationResult.Error != null)
            return new(default, operationResult.Error);

        var user = _contextAccessor.HttpContext?.User;

        if (user == null)
            return new OperationResult<DomainUser>(default, new Error("User is not logged in!", ErrorType.AuthorizationIssue));

        var id = user.Claims.Single(x => x.Type == ClaimsTypes.Id).Value;
        var username = user.Claims.Single(x => x.Type == ClaimsTypes.UserName).Value;
        var isAdminLiteral = user.Claims.Single(x => x.Type == ClaimsTypes.IsAdmin).Value;
        var isAdmin = bool.Parse(isAdminLiteral);
        var hasBasicAuthSetupLiteral = user.Claims.Single(x => x.Type == ClaimsTypes.HasBasicAuth).Value;
        var hasBasicAuthSetup = bool.Parse(hasBasicAuthSetupLiteral);
        var tenantIdLiteral = user.Claims.Single(x => x.Type == ClaimsTypes.Tenant).Value;
        var tenantId = int.Parse(tenantIdLiteral, CultureInfo.InvariantCulture);

        var result = new DomainUser(id, username, isAdmin, hasBasicAuthSetup, tenantId);
        return new OperationResult<DomainUser>(result, null);
    }
}