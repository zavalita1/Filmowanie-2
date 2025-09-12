using System.Globalization;
using System.Security.Claims;
using Filmowanie.Abstractions.Constants;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Account.Constants;
using Filmowanie.Account.Interfaces;
using Filmowanie.Account.Models;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Microsoft.AspNetCore.Authentication;

namespace Filmowanie.Account.Helpers;

internal sealed class LoginResultDataExtractor : ILoginResultDataExtractor
{
    public Maybe<LoginResultData> GetIdentity(IReadOnlyUserEntity user)
    {
        var hasBasicAuth = !string.IsNullOrEmpty(user.PasswordHash);
        var claims = new[]
        {
            new Claim(ClaimsTypes.UserName, user.DisplayName),
            new Claim(ClaimsTypes.UserId, user.id),
            new Claim(ClaimsTypes.IsAdmin, user.IsAdmin.ToString(CultureInfo.InvariantCulture)),
            new Claim(ClaimsTypes.Tenant, user.TenantId.ToString()),
            new Claim(ClaimsTypes.HasBasicAuth, hasBasicAuth.ToString(CultureInfo.InvariantCulture)),
            new Claim(ClaimsTypes.Created, user.Created.ToString("O")),
        };

        var claimsIdentity = new ClaimsIdentity(claims, Schemes.Cookie);
        var authProps = new AuthenticationProperties
        {
            AllowRefresh = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(Authorization.AuthorizationExpirationTimeInDays),
            IsPersistent = true,
            IssuedUtc = DateTimeOffset.UtcNow,
        };

        return new Maybe<LoginResultData>(new LoginResultData(claimsIdentity, authProps), null);
    }
}