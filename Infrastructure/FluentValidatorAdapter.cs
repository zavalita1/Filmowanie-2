using System.Linq;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Infrastructure;

internal sealed class FluentValidatorAdapter<TInput> : IFluentValidatorAdapter<TInput>
{
    private readonly IValidator<TInput> validator;
    private readonly ILogger<FluentValidatorAdapter<TInput>> log;

    public FluentValidatorAdapter(IValidator<TInput> validator, ILogger<FluentValidatorAdapter<TInput>> log)
    {
        this.validator = validator;
        this.log = log;
    }

    public Maybe<TInput> Validate(Maybe<TInput> maybeInput) => maybeInput.Accept(Validate, this.log);

    public Maybe<TInput> Validate(TInput input)
    {
        var fluentResult = this.validator.Validate(input);

        if (fluentResult.IsValid)
            return new Maybe<TInput>(input, null);

        var errorMessages = fluentResult.Errors.Select(x => x.ErrorMessage);
        var error = new Error<TInput>(errorMessages, ErrorType.ValidationError);
        return new Maybe<TInput>(input, error);
    }
}