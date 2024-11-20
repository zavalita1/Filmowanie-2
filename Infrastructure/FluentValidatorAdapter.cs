using System;
using System.Linq;
using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Interfaces;
using FluentValidation;

namespace Filmowanie.Infrastructure;

public sealed class FluentValidatorAdapter<TInput> : IFluentValidatorAdapter<TInput>
{
    private readonly IValidator<TInput> _validator;

    public FluentValidatorAdapter(IValidator<TInput> validator)
    {
        _validator = validator;
    }

    public OperationResult<TInput> Validate(TInput input)
    {
        var fluentResult = _validator.Validate(input);

        if (fluentResult.IsValid)
            return new OperationResult<TInput>(input, null);

        var errorMessages = fluentResult.Errors.Select(x => x.ErrorMessage);
        var errorMessage = string.Join(',', errorMessages);
        var error = new Error(errorMessage, ErrorType.ValidationError);
        return new OperationResult<TInput>(input, error);
    }
}