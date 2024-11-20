using System;
using System.Linq;
using System.Threading.Tasks;
using Filmowanie.Account.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Filters
{
    public sealed class LoggingActionFilter : IEndpointFilter
    {
        private readonly ILogger<LoggingActionFilter> _log;

        public LoggingActionFilter(ILogger<LoggingActionFilter> log)
        {
            _log = log;
        }

        public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var displayName = context.HttpContext.Request.Path;
            var userId = context.HttpContext.User.Claims.SingleOrDefault(x => x.Type == ClaimsTypes.UserId)?.Value;
            _log.LogInformation("{} [Request {}, UserId: {}] Request starting", DateTimeOffset.UtcNow, displayName, userId);
            var result = await next(context);
            _log.LogInformation("{} [Request {}, UserId: {}] Request ending. Result: {}", DateTimeOffset.UtcNow, displayName, userId, (result as IStatusCodeHttpResult)?.StatusCode);
            return result;
        }
    }
}
