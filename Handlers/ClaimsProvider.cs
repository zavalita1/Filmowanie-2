using System;
using System.Globalization;
using System.Security.Claims;
using Filmowanie.Constants;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Handlers;

public class ClaimsProvider
{
    public static ClaimsIdentity GetClaims(IResponseCookies cookies, string code)
    {
        var claims = GetClaimsIdentity(code);

        if (claims == null)
            throw new UnauthorizedAccessException();

        return claims;
    }

    private static ClaimsIdentity GetClaimsIdentity(string code)
    {
        if (!UsersCache.TryGetUser(code, out var user))
            return null;

        var claims = new[]
        {
            new Claim(ClaimsTypes.UserName, user.Username),
            new Claim(ClaimsTypes.UserId, user.UserId.ToString()),
            new Claim(ClaimsTypes.IsAdmin, user.IsAdmin.ToString(CultureInfo.InvariantCulture)),
            new Claim(ClaimsTypes.Code, code),
            new Claim(ClaimsTypes.Tenant, user.TenantId.ToString()),
        };

        var identity = new ClaimsIdentity(claims, nameof(ClaimsProvider));
        return identity;
    }
}

internal static class UsersCache
{
    public static bool TryGetUser(string code, out (string Username, bool IsAdmin, int UserId, int TenantId) user) => throw new NotImplementedException();

    public static void HydrateCache()
    {
        // TODO
    }
}