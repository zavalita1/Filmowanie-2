using System;
using System.Collections.Generic;
using Filmowanie.Abstractions.Interfaces;
using FluentValidation;

namespace Filmowanie.Infrastructure;

internal sealed class FluentValidatorAdapterProvider : IFluentValidatorAdapterProvider
{
    private readonly IEnumerable<IFluentValidatorAdapter> validators;
    private readonly IFluentValidationAdapterFactory factory;

    public FluentValidatorAdapterProvider(IEnumerable<IFluentValidatorAdapter> validators, IFluentValidationAdapterFactory factory)
    {
        this.validators = validators;
        this.factory = factory;
    }

    public IFluentValidatorAdapter<TInput> GetAdapter<TInput>(string keyedInstance)
    {
        IValidator<TInput>? typedValidator = null!;

        foreach (var validator in this.validators)
        {
            if (validator.CanHandle(keyedInstance, out typedValidator))
                break;
        }

        if (typedValidator == null)
            throw new InvalidOperationException($"No registered validator found for type: {typeof(TInput).Name} and key: {keyedInstance}");

        var adapter = this.factory.Create(typedValidator);
        return adapter;
    }

    public IFluentValidatorAdapter<TInput> GetAdapter<TInput>() => GetAdapter<TInput>(string.Empty);
}