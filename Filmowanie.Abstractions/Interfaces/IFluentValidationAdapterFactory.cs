using FluentValidation;

namespace Filmowanie.Abstractions.Interfaces;

public interface IFluentValidationAdapterFactory
{
    IFluentValidatorAdapter<T> Create<T>(IValidator<T> innerValidator);
}