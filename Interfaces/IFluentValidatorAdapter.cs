using Filmowanie.Abstractions;
using FluentValidation;

namespace Filmowanie.Interfaces;

public interface IFluentValidatorAdapter<TInput>
{
    public OperationResult<TInput> Validate(TInput input);
}

public interface IFluentValidatorAdapter
{
    public bool CanHandle<T>(string key, out IValidator<T> typedValidator);
}