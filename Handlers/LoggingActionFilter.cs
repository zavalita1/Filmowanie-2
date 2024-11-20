using System;
using System.Linq;
using System.Threading.Tasks;
using Filmowanie.Account.Constants;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Handlers
{
    public sealed class LoggingActionFilter : IAsyncActionFilter
    {
        private readonly ILogger<LoggingActionFilter> _log;

        public LoggingActionFilter(ILogger<LoggingActionFilter> log)
        {
            _log = log;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var displayName = context.ActionDescriptor.DisplayName;
            var userId = context.HttpContext.User.Claims.SingleOrDefault(x => x.Type == ClaimsTypes.UserId)?.Value;
            _log.LogInformation("{} [Request {}, UserId: {}] Request starting",  DateTimeOffset.UtcNow, displayName, userId);
            var result = await next();
            _log.LogInformation("{} [Request {}, UserId: {}] Request ending. Result: {}", DateTimeOffset.UtcNow, displayName, userId, result.HttpContext.Response.StatusCode);
        }
    }
}
