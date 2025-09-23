using System.Security.Claims;
using Filmowanie.Account.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Account.Helpers;

internal sealed class HttpContextWrapper : IHttpContextWrapper
{
    private readonly IHttpContextAccessor contextAccessor;

    public HttpContextWrapper(IHttpContextAccessor contextAccessor)
    {
        this.contextAccessor = contextAccessor;
    }

    public async Task SignInAsync(string scheme, ClaimsPrincipal claimsPrincipal, AuthenticationProperties properties)
    {
        await this.contextAccessor.HttpContext!.SignInAsync(scheme, claimsPrincipal, properties);
        this.contextAccessor.HttpContext!.User = claimsPrincipal;
    }

    public Task SignOutAsync(string scheme) => this.contextAccessor.HttpContext!.SignOutAsync(scheme);

    public ClaimsPrincipal? User => this.contextAccessor.HttpContext?.User;
    
    public HttpRequest? Request => this.contextAccessor.HttpContext?.Request;
}