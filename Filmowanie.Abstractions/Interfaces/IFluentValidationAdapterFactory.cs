using Filmowanie.Abstractions.Interfaces;
using FluentValidation;

namespace Filmowanie.Interfaces;

public interface IFluentValidationAdapterFactory
{
    IFluentValidatorAdapter<T> Create<T>(IValidator<T> innerValidator);
}