using System.Globalization;
using System.Security.Claims;
using Filmowanie.Abstractions.Constants;
using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.Constants;
using Filmowanie.Account.Interfaces;
using Filmowanie.Account.Models;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Account.Services;

internal sealed class AuthenticationManager : IAuthenticationManager
{
    private readonly ILogger<AuthenticationManager> log;
    private readonly IHttpContextWrapper httpContextWrapper;

    public AuthenticationManager(ILogger<AuthenticationManager> log, IHttpContextWrapper httpContextWrapper)
    {
        this.log = log;
        this.httpContextWrapper = httpContextWrapper;
    }

    public Task<Maybe<VoidResult>> LogInAsync(Maybe<LoginResultData> maybeLoginData, CancellationToken cancelToken) => maybeLoginData.AcceptAsync(LogInAsync, this.log, cancelToken);
    public Task<Maybe<VoidResult>> LogOutAsync(Maybe<VoidResult> maybe, CancellationToken cancelToken) => maybe.AcceptAsync(LogOutAsync, this.log, cancelToken);

    public Maybe<DomainUser> GetDomainUser(Maybe<VoidResult> maybe) => maybe.Accept(GetDomainUser, this.log);


    private Maybe<DomainUser> GetDomainUser()
    {
        var user = this.httpContextWrapper.User;

        if (user == null || user.Identity?.IsAuthenticated != true)
        {
            var errors = this.httpContextWrapper.Request!.Cookies.ContainsKey(".AspNetCore.cookie")
                ? (string[])[Messages.CookieExpired, Messages.UserNotLoggerIn]
                : [Messages.UserNotLoggerIn];

            return new Error<DomainUser>(errors, ErrorType.AuthenticationIssue);
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
        var genderLiteral = user.Claims.Single(x => x.Type == ClaimsTypes.Gender).Value;
        var gender = Enum.Parse<Gender>(genderLiteral);

        var result = new DomainUser(id, username, isAdmin, hasBasicAuthSetup, tenant, created, gender);
        return new Maybe<DomainUser>(result, null);
    }

    private async Task<Maybe<VoidResult>> LogInAsync(LoginResultData loginData, CancellationToken cancelToken)
    {
        log.LogInformation("Logging in...");
        var claimsPrincipal = new ClaimsPrincipal(loginData.Identity);
        await this.httpContextWrapper.SignInAsync(Schemes.Cookie, claimsPrincipal, loginData.AuthenticationProperties);
        log.LogInformation("Logged in!");
        return VoidResult.Void;
    }

    private async Task<Maybe<VoidResult>> LogOutAsync(CancellationToken cancelToken)
    {
        await this.httpContextWrapper.SignOutAsync(Schemes.Cookie);
        return VoidResult.Void;
    }
}