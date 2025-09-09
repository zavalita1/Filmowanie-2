using Filmowanie.Abstractions.Maybe;
using FluentValidation;

namespace Filmowanie.Abstractions.Interfaces;

public interface IFluentValidatorAdapter<TInput>
{
    public Maybe<TInput> Validate(TInput input);
    public Maybe<TInput> Validate(Maybe<TInput> input);
}

public interface IFluentValidatorAdapter
{
    public bool CanHandle<T>(string key, out IValidator<T>? typedValidator);
}