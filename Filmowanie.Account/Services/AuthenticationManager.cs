using System.Globalization;
using System.Security.Claims;
using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Constants;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.Constants;
using Filmowanie.Account.Interfaces;
using Filmowanie.Account.Results;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Account.Services;

internal sealed class AuthenticationManager : IAuthenticationManager
{
    private readonly ILogger<AuthenticationManager> _log;
    private readonly IHttpContextWrapper _httpContextWrapper;

    public AuthenticationManager(ILogger<AuthenticationManager> log, IHttpContextWrapper httpContextWrapper)
    {
        _log = log;
        _httpContextWrapper = httpContextWrapper;
    }

    public Task<OperationResult<VoidResult>> LogInAsync(OperationResult<LoginResultData> maybeLoginData, CancellationToken cancelToken) => maybeLoginData.AcceptAsync(LogInAsync, _log, cancelToken);
    public Task<OperationResult<VoidResult>> LogOutAsync(OperationResult<VoidResult> maybe, CancellationToken cancelToken) => maybe.AcceptAsync(LogOutAsync, _log, cancelToken);

    public OperationResult<DomainUser> GetDomainUser(OperationResult<VoidResult> maybe) => maybe.Accept(GetDomainUser, _log);


    private OperationResult<DomainUser> GetDomainUser()
    {
        var user = _httpContextWrapper.User;

        if (user == null || user.Identity?.IsAuthenticated != true)
        {
            var errors = _httpContextWrapper.Request!.Cookies.ContainsKey(".AspNetCore.cookie")
                ? (string[])[Messages.CookieExpired, Messages.UserNotLoggerIn]
                : [Messages.UserNotLoggerIn];

            return new Error(errors, ErrorType.AuthenticationIssue).ToOperationResult<DomainUser>();
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

    private async Task<OperationResult<VoidResult>> LogInAsync(LoginResultData loginData, CancellationToken cancelToken)
    {
        _log.LogInformation("Logging in...");
        var claimsPrincipal = new ClaimsPrincipal(loginData.Identity);
        await _httpContextWrapper.SignInAsync(Schemes.Cookie, claimsPrincipal, loginData.AuthenticationProperties);
        _log.LogInformation("Logged in!");
        return OperationResultExtensions.Void;
    }

    private async Task<OperationResult<VoidResult>> LogOutAsync(CancellationToken cancelToken)
    {
        await _httpContextWrapper.SignOutAsync(Schemes.Cookie);
        return OperationResultExtensions.Void;
    }
}