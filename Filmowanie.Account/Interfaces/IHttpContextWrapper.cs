using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Account.Interfaces;

internal interface IHttpContextWrapper
{
    Task SignInAsync(string scheme, ClaimsPrincipal claimsPrincipal, AuthenticationProperties properties);
    Task SignOutAsync(string scheme);
    
    ClaimsPrincipal? User { get; }
    HttpRequest? Request { get; }
}