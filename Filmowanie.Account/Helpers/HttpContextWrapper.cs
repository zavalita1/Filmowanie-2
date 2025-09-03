using System.Security.Claims;
using Filmowanie.Account.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Account.Helpers;

internal sealed class HttpContextWrapper : IHttpContextWrapper
{
    private readonly IHttpContextAccessor _contextAccessor;

    public HttpContextWrapper(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public async Task SignInAsync(string scheme, ClaimsPrincipal claimsPrincipal, AuthenticationProperties properties)
    {
        await _contextAccessor.HttpContext!.SignInAsync(scheme, claimsPrincipal, properties);
        _contextAccessor.HttpContext!.User = claimsPrincipal;
    }

    public Task SignOutAsync(string scheme) => _contextAccessor.HttpContext!.SignOutAsync(scheme);

    public ClaimsPrincipal? User => _contextAccessor.HttpContext?.User;
    
    public HttpRequest? Request => _contextAccessor.HttpContext?.Request;
}