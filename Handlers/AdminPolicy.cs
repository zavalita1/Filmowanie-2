using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Filmowanie.Handlers;

public sealed class AdminAccessRequirement : AuthorizationHandler<AdminAccessRequirement>, IAuthorizationRequirement
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminAccessRequirement requirement)
    {
        var isAdminClaim = context.User.Claims.Single(x => x.Type.Equals("isAdmin", StringComparison.Ordinal));
        var isAdmin = bool.Parse(isAdminClaim.Value);

        if (isAdmin)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}