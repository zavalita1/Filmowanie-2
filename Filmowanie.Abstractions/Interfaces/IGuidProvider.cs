namespace Filmowanie.Abstractions.Interfaces;

/// <summary>
/// Provides a way to generate new GUIDs.
/// This abstraction allows for better testability by making GUID generation controllable in tests.
/// </summary>
public interface IGuidProvider
{
    /// <summary>
    /// Generates a new GUID.
    /// In production, this generates a new random GUID.
    /// In tests, this can be controlled to return specific GUIDs.
    /// </summary>
    /// <returns>A new GUID.</returns>
    Guid NewGuid();
}