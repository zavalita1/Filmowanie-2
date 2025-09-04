using Filmowanie.Abstractions.OperationResult;
using FluentValidation;

namespace Filmowanie.Abstractions.Interfaces;

public interface IFluentValidatorAdapter<TInput>
{
    public OperationResult<TInput> Validate(TInput input);
    public OperationResult<TInput> Validate(OperationResult<TInput> input);
}

public interface IFluentValidatorAdapter
{
    public bool CanHandle<T>(string key, out IValidator<T>? typedValidator);
}