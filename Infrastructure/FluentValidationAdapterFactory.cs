using System;
using Filmowanie.Abstractions.Interfaces;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Infrastructure;

internal sealed class FluentValidationAdapterFactory : IFluentValidationAdapterFactory
{
    private readonly IServiceProvider serviceProvider;

    public FluentValidationAdapterFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public IFluentValidatorAdapter<T> Create<T>(IValidator<T> innerValidator)
    {
        var log = this.serviceProvider.GetRequiredService<ILogger<FluentValidatorAdapter<T>>>();
        return new FluentValidatorAdapter<T>(innerValidator, log);
    }
}