using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Tests.TestHelpers;

internal sealed class LoggerForTests<T> : ILogger<T>
{
    private readonly ConcurrentBag<(LogLevel, string, Exception?)> _received = new();

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        throw new NotSupportedException("Don't use this");
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _received.Add((logLevel, formatter(state, exception), exception));
    }

    public bool Received(LogLevel level, string messageSubstring) => _received.Any(x => x.Item1 == level && x.Item2.Contains(messageSubstring));

    public int TotalReceived => _received.Count;
}