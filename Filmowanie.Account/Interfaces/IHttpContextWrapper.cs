using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Filmowanie.Account.Interfaces;

public interface IHttpContextWrapper
{
    Task SignInAsync(string scheme, ClaimsPrincipal claimsPrincipal, AuthenticationProperties properties);
    Task SignOutAsync(string scheme);
}