using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Filmowanie.Extensions.Initialization;

internal static class LoggingInit
{
    public static void ConfigureLogging(this WebApplicationBuilder appBuilder)
    {
        appBuilder.Logging.ClearProviders();
        appBuilder.Logging.AddZLoggerConsole();
        var currentDll = Assembly.GetExecutingAssembly().Location;
        var currentDir = Path.GetDirectoryName(currentDll);
        var logPath = $"{currentDir}\\AppLog.txt";
        appBuilder.Logging.AddZLoggerFile(logPath);
    }
}