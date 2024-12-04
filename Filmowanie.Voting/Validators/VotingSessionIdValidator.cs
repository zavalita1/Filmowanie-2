using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Voting.Constants;
using FluentValidation;

namespace Filmowanie.Voting.Validators;

internal class VotingSessionIdValidator : AbstractValidator<string>, IFluentValidatorAdapter
{
    public VotingSessionIdValidator()
    {
        RuleFor(x => x).Must(x => Guid.TryParse(x, out _)).When(x => !string.IsNullOrEmpty(x)).WithMessage($"Value must be a valid guid!");
    }

    public bool CanHandle<T>(string key, out IValidator<T>? typedValidator)
    {
        typedValidator = null;
        if (typeof(T) == typeof(string) && key == KeyedServices.VotingSessionId)
        {
            typedValidator = (IValidator<T>)this;
            return true;
        }

        return false;
    }
}