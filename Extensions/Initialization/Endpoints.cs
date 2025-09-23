using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
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
    public static void ConfigureEndpoints(this WebApplication webApplication)
    {
        var apiGroup = webApplication.MapGroup("api");
        apiGroup.AddEndpointFilter<LoggingActionFilter>();
        apiGroup.RegisterAccountRoutes();
        apiGroup.RegisterVotingRoutes();
        apiGroup.RegisterNominationRoutes();
        apiGroup.RegisterNotificationRoutes();

        EnvironmentDependent.Invoke(new ()
        {
            [StartupMode.Local] = () =>
            {
                webApplication.UseSwagger();
                webApplication.UseSwaggerUI();
            }
        });

        webApplication.UseWhen(
            context => !context.Request.Path.StartsWithSegments("/api"),
            c =>
            {
                c.UseSpa(spa =>
                {
                    EnvironmentDependent.Invoke(new()
                    {
                        [StartupMode.LocalWithDevFrontend] = () => spa.UseProxyToSpaDevelopmentServer(webApplication.Configuration["FrontendDevServer"]),
                        [StartupMode.LocalWithCompiledFrontend | StartupMode.LocalWithCosmosEmulatorAndCompiledFrontend] = () => spa.Options.SourcePath = "wwwroot",
                        [StartupMode.Production] = () => spa.Options.SourcePath = string.Empty
                    });
                });
            });
    }
}