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
        appBuilder.Logging.AddZLoggerConsole(o => o.UsePlainTextFormatter(formatter =>
        {
            formatter.SetPrefixFormatter($"{0} | {1} | {2} ", (in MessageTemplate template, in LogInfo info) => template.Format(info.Timestamp, info.LogLevel, info.Category));
        }));
        var currentDll = Assembly.GetExecutingAssembly().Location;
        var currentDir = Path.GetDirectoryName(currentDll);
        var logPath = $"{currentDir}\\AppLog.txt";
        appBuilder.Logging.AddZLoggerRollingFile(o =>
        {
            o.FilePathSelector = (dt, index) => $"AppLogs-{dt:yy-MM-dd}_{index}.log";
            o.RollingInterval = ZLogger.Providers.RollingInterval.Day;
            o.RollingSizeKB = 1024;
            o.CaptureThreadInfo = true;
        });
    }
}