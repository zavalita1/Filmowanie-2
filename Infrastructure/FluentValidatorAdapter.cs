using System.Linq;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
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

    public Maybe<TInput> Validate(Maybe<TInput> maybeInput) => maybeInput.Accept(Validate, _log);

    public Maybe<TInput> Validate(TInput input)
    {
        var fluentResult = _validator.Validate(input);

        if (fluentResult.IsValid)
            return new Maybe<TInput>(input, null);

        var errorMessages = fluentResult.Errors.Select(x => x.ErrorMessage);
        var error = new Error<TInput>(errorMessages, ErrorType.ValidationError);
        return new Maybe<TInput>(input, error);
    }
}