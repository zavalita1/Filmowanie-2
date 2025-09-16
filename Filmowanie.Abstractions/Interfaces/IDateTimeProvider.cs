namespace Filmowanie.Abstractions.Interfaces;

/// <summary>
/// Provides a way to access the current date and time.
/// This abstraction allows for better testability by making datetime operations controllable in tests.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current date and time.
    /// In production, this returns the actual system time.
    /// In tests, this can be controlled to return specific timestamps.
    /// </summary>
    DateTime Now { get; }
}