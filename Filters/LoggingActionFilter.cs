using System;
using System.Linq;
using System.Threading.Tasks;
using Filmowanie.Account.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Filters;

public sealed class LoggingActionFilter : IEndpointFilter
{
    private readonly ILogger<LoggingActionFilter> log;

    public LoggingActionFilter(ILogger<LoggingActionFilter> log)
    {
        this.log = log;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var displayName = context.HttpContext.Request.Path;
        var userId = context.HttpContext.User.Claims.SingleOrDefault(x => x.Type == ClaimsTypes.UserId)?.Value;
        this.log.LogInformation("{} [Request {} {}, UserId: {}] Request starting", DateTimeOffset.UtcNow, context.HttpContext.Request.Method, displayName, userId);
        var result = await next(context);
        this.log.LogInformation("{} [Request {} {}, UserId: {}] Request ending. Result: {}", DateTimeOffset.UtcNow, context.HttpContext.Request.Method, displayName, userId, (result as IStatusCodeHttpResult)?.StatusCode);
        return result;
    }
}