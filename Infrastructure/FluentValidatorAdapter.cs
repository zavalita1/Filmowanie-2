using System.Linq;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Infrastructure;

public sealed class FluentValidatorAdapter<TInput> : IFluentValidatorAdapter<TInput>
{
    private readonly IValidator<TInput> _validator;
    private readonly ILogger<FluentValidatorAdapter<TInput>> _log;

    public FluentValidatorAdapter(IValidator<TInput> validator, ILogger<FluentValidatorAdapter<TInput>> log)
    {
        _validator = validator;
        _log = log;
    }

    public OperationResult<TInput> Validate(OperationResult<TInput> maybeInput) => maybeInput.Accept(Validate, _log);

    public OperationResult<TInput> Validate(TInput input)
    {
        var fluentResult = _validator.Validate(input);

        if (fluentResult.IsValid)
            return new OperationResult<TInput>(input, null);

        var errorMessages = fluentResult.Errors.Select(x => x.ErrorMessage);
        var error = new Error(errorMessages, ErrorType.ValidationError);
        return new OperationResult<TInput>(input, error);
    }
}