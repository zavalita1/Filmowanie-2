using System.Globalization;
using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Account.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Account.Visitors;

internal sealed class UserIdentityVisitor : IUserIdentityVisitor
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly ILogger<UserIdentityVisitor> _log;

    public UserIdentityVisitor(IHttpContextAccessor contextAccessor, ILogger<UserIdentityVisitor> log)
    {
        _contextAccessor = contextAccessor;
        _log = log;
    }

    public OperationResult<DomainUser> Visit<T>(OperationResult<T> operationResult)
    {
        var user = _contextAccessor.HttpContext?.User;

        if (user == null || user.Identity?.IsAuthenticated != true)
        {
            var errors = _contextAccessor.HttpContext!.Request.Cookies.ContainsKey(".AspNetCore.cookie") ? (string[])[Messages.CookieExpired, Messages.UserNotLoggerIn] : [Messages.UserNotLoggerIn];
            return new OperationResult<DomainUser>(default, new Error(errors, ErrorType.AuthenticationIssue));
        }
            

        var id = user.Claims.Single(x => x.Type == ClaimsTypes.UserId).Value;
        var username = user.Claims.Single(x => x.Type == ClaimsTypes.UserName).Value;
        var isAdminLiteral = user.Claims.Single(x => x.Type == ClaimsTypes.IsAdmin).Value;
        var isAdmin = bool.Parse(isAdminLiteral);
        var hasBasicAuthSetupLiteral = user.Claims.Single(x => x.Type == ClaimsTypes.HasBasicAuth).Value;
        var hasBasicAuthSetup = bool.Parse(hasBasicAuthSetupLiteral);
        var tenantIdLiteral = user.Claims.Single(x => x.Type == ClaimsTypes.Tenant).Value;
        var tenantId = int.Parse(tenantIdLiteral, CultureInfo.InvariantCulture);
        var tenant = new TenantId(tenantId);
        var createdLiteral = user.Claims.Single(x => x.Type == ClaimsTypes.Created).Value;
        var created = DateTime.Parse(createdLiteral, null, DateTimeStyles.RoundtripKind);

        var result = new DomainUser(id, username, isAdmin, hasBasicAuthSetup, tenant, created);
        return new OperationResult<DomainUser>(result, null);
    }

    public ILogger Log => _log;
}