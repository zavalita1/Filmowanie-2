using Filmowanie.Abstractions.Interfaces;

namespace Filmowanie.Abstractions.Wrappers;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.UtcNow;
}