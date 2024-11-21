using Filmowanie.Abstractions.Interfaces;

namespace Filmowanie.Abstractions.Providers;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.UtcNow;
}