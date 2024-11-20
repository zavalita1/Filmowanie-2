using System;
using System.Collections.Generic;
using Filmowanie.Abstractions.Interfaces;
using FluentValidation;

namespace Filmowanie.Infrastructure;

public sealed class FluentValidatorAdapterFactory : IFluentValidatorAdapterFactory
{
    private readonly IEnumerable<IFluentValidatorAdapter> _validators;

    public FluentValidatorAdapterFactory(IEnumerable<IFluentValidatorAdapter> validators)
    {
        _validators = validators;
    }

    public IFluentValidatorAdapter<TInput> GetAdapter<TInput>(string keyedInstance)
    {
        var typedValidator = (IValidator<TInput>)null;

        foreach (var validator in _validators)
        {
            if (validator.CanHandle(keyedInstance, out typedValidator))
                break;
        }

        if (typedValidator == null)
            throw new InvalidOperationException($"No registered validator found for type: {nameof(TInput)} and key: {keyedInstance}");

        var adapter = new FluentValidatorAdapter<TInput>(typedValidator);
        return adapter;
    }

    public IFluentValidatorAdapter<TInput> GetAdapter<TInput>() => GetAdapter<TInput>(string.Empty);
}