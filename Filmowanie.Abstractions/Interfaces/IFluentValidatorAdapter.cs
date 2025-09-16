using Filmowanie.Abstractions.Maybe;
using FluentValidation;

namespace Filmowanie.Abstractions.Interfaces;

/// <summary>
/// Provides a generic adapter for FluentValidation validators, wrapping validation results in Maybe monads.
/// This interface is used to standardize validation across the application while maintaining type safety.
/// </summary>
/// <typeparam name="TInput">The type of the input to validate.</typeparam>
public interface IFluentValidatorAdapter<TInput>
{
    /// <summary>
    /// Validates the provided input and wraps the result in a Maybe monad.
    /// </summary>
    /// <param name="input">The input to validate.</param>
    /// <returns>A Maybe monad containing either the validated input or validation errors.</returns>
    public Maybe<TInput> Validate(TInput input);

    /// <summary>
    /// Validates an already Maybe-wrapped input, maintaining the Maybe context.
    /// </summary>
    /// <param name="input">The Maybe-wrapped input to validate.</param>
    /// <returns>A Maybe monad containing either the validated input or validation errors.</returns>
    public Maybe<TInput> Validate(Maybe<TInput> input);
}

/// <summary>
/// Provides a way to check if a validator can handle a specific type with a given key.
/// This interface is used in the validator resolution system.
/// </summary>
public interface IFluentValidatorAdapter
{
    /// <summary>
    /// Determines if this adapter can handle validation for a specific type and key combination.
    /// </summary>
    /// <typeparam name="T">The type to validate.</typeparam>
    /// <param name="key">The key identifying the specific validation context.</param>
    /// <param name="typedValidator">When method returns true, contains the validator for type T; otherwise, null.</param>
    /// <returns>True if this adapter can handle the specified type and key; otherwise, false.</returns>
    public bool CanHandle<T>(string key, out IValidator<T>? typedValidator);
}