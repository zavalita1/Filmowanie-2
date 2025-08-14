using Filmowanie.Abstractions.Enums;
using Filmowanie.Account.Extensions;
using Filmowanie.Filters;
using Filmowanie.Nomination.Extensions;
using Filmowanie.Notification.Extensions;
using Filmowanie.Voting.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Extensions.Initialization;

internal static class Endpoints
{
    public static void ConfigureEndpoints(this WebApplication webApplication, Environment appEnvironment)
    {
        var apiGroup = webApplication.MapGroup("api");
        apiGroup.AddEndpointFilter<LoggingActionFilter>();
        apiGroup.RegisterAccountRoutes();
        apiGroup.RegisterVotingRoutes();
        apiGroup.RegisterNominationRoutes();
        apiGroup.RegisterNotificationRoutes();

        webApplication.UseWhen(
            context => !context.Request.Path.StartsWithSegments("/api"),
            c =>
            {
                c.UseSpa(spa =>
                {
                    if (appEnvironment == Environment.Development)
                    {
                        var devServerUrl = webApplication.Configuration["FrontendDevServer"];
                        spa.UseProxyToSpaDevelopmentServer(devServerUrl);
                    }
                });
            });

        if (appEnvironment == Environment.Development)
        {
            webApplication.UseSwagger();
            webApplication.UseSwaggerUI();
        }
    }
}