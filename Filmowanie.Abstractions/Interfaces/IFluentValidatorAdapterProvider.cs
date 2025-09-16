namespace Filmowanie.Abstractions.Interfaces;

/// <summary>
/// Provides a factory service for retrieving appropriate FluentValidator adapters.
/// This service is used to resolve and create validator adapters based on input types and optional keys.
/// </summary>
public interface IFluentValidatorAdapterProvider
{
    /// <summary>
    /// Gets a validator adapter for a specific input type and key combination.
    /// </summary>
    /// <typeparam name="TInput">The type of input to validate.</typeparam>
    /// <param name="keyedInstance">A key to identify a specific validator for the input type.</param>
    /// <returns>A validator adapter that can validate the specified input type.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no suitable validator is found.</exception>
    public IFluentValidatorAdapter<TInput> GetAdapter<TInput>(string keyedInstance);

    /// <summary>
    /// Gets a validator adapter for a specific input type using an empty key.
    /// </summary>
    /// <typeparam name="TInput">The type of input to validate.</typeparam>
    /// <returns>A validator adapter that can validate the specified input type.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no suitable validator is found.</exception>
    public IFluentValidatorAdapter<TInput> GetAdapter<TInput>();
}