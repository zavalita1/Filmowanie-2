using System;
using System.Collections.Generic;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Interfaces;
using FluentValidation;

namespace Filmowanie.Infrastructure;

public sealed class FluentValidatorAdapterProvider : IFluentValidatorAdapterProvider
{
    private readonly IEnumerable<IFluentValidatorAdapter> _validators;
    private readonly IFluentValidationAdapterFactory _factory;

    public FluentValidatorAdapterProvider(IEnumerable<IFluentValidatorAdapter> validators, IFluentValidationAdapterFactory factory)
    {
        _validators = validators;
        _factory = factory;
    }

    public IFluentValidatorAdapter<TInput> GetAdapter<TInput>(string keyedInstance)
    {
        var typedValidator = (IValidator<TInput>)null!;

        foreach (var validator in _validators)
        {
            if (validator.CanHandle(keyedInstance, out typedValidator))
                break;
        }

        if (typedValidator == null)
            throw new InvalidOperationException($"No registered validator found for type: {typeof(TInput).Name} and key: {keyedInstance}");

        var adapter = _factory.Create(typedValidator);
        return adapter;
    }

    public IFluentValidatorAdapter<TInput> GetAdapter<TInput>() => GetAdapter<TInput>(string.Empty);
}