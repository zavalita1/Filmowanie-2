using System;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Interfaces;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Infrastructure;

public sealed class FluentValidationAdapterFactory : IFluentValidationAdapterFactory
{
    private readonly IServiceProvider _serviceProvider;

    public FluentValidationAdapterFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IFluentValidatorAdapter<T> Create<T>(IValidator<T> innerValidator)
    {
        var log = _serviceProvider.GetRequiredService<ILogger<FluentValidatorAdapter<T>>>();
        return new FluentValidatorAdapter<T>(innerValidator, log);
    }
}