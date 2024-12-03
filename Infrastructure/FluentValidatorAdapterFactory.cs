using System;
using System.Collections.Generic;
using Filmowanie.Abstractions.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Infrastructure;

public sealed class FluentValidatorAdapterFactory : IFluentValidatorAdapterFactory
{
    private readonly IEnumerable<IFluentValidatorAdapter> _validators;
    private readonly IServiceProvider _serviceProvider;

    public FluentValidatorAdapterFactory(IEnumerable<IFluentValidatorAdapter> validators, IServiceProvider serviceProvider)
    {
        _validators = validators;
        _serviceProvider = serviceProvider;
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

        var adapter = new FluentValidatorAdapter<TInput>(typedValidator, _serviceProvider.GetRequiredService<ILogger<FluentValidatorAdapter<TInput>>>());
        return adapter;
    }

    public IFluentValidatorAdapter<TInput> GetAdapter<TInput>() => GetAdapter<TInput>(string.Empty);
}