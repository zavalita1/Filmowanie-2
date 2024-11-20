using System;
using System.Globalization;
using System.Linq;
using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Account.Constants;
using Filmowanie.Account.Interfaces;
using Filmowanie.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Account.Visitors;

public sealed class UserIdentityVisitor : IUserIdentityVisitor
{
    private readonly IHttpContextAccessor _contextAccessor;

    public UserIdentityVisitor(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public OperationResult<DomainUser> Visit<T>(OperationResult<T> operationResult)
    {
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
        var createdLiteral = user.Claims.Single(x => x.Type == ClaimsTypes.Created).Value;
        var created = DateTime.Parse(createdLiteral, null, DateTimeStyles.RoundtripKind);

        var result = new DomainUser(id, username, isAdmin, hasBasicAuthSetup, tenantId, created);
        return new OperationResult<DomainUser>(result, null);
    }
}