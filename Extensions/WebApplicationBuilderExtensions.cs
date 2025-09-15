using System;
using Filmowanie.Abstractions.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Filmowanie.Extensions;

public static class WebApplicationBuilderExtensions
{
    private static StartupMode GetStartupMode(this WebApplicationBuilder builder)
    {
        var startupMode = builder.Configuration["StartupMode"];
        if (!string.IsNullOrEmpty(startupMode) && Enum.TryParse<StartupMode>(startupMode, out var mode))
            return mode;

        return builder.Environment.IsDevelopment() ? StartupMode.Local : StartupMode.Production;
    }

    public static void SetStartupMode(this WebApplicationBuilder builder)
    {
        var result = builder.GetStartupMode();
        Environment.Mode = result;
    }
}