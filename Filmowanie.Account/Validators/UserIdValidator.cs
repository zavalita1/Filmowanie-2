using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Account.Constants;
using FluentValidation;

namespace Filmowanie.Account.Validators;

internal class UserIdValidator : AbstractValidator<string>, IFluentValidatorAdapter
{
    public UserIdValidator()
    {
        RuleFor(x => x).NotNull().WithMessage("Value cannot be null!");
        RuleFor(x => x).NotEmpty().WithMessage("Value cannot be empty!");
        RuleFor(x => x).Must(x => x.StartsWith("user-")).WithMessage("Must start with proper prefix!");
        RuleFor(x => x).Length(6, 9999).WithMessage("Length must be between 6 and 9999 characters");
        When(x => x.Length > 5, () => RuleFor(x => x.Substring(5)).Must(x => Guid.TryParse(x, out var guid)).WithMessage("Must be guid-parsable!"));
    }

    public bool CanHandle<T>(string key, out IValidator<T>? typedValidator)
    {
        typedValidator = null;
        if (typeof(T) == typeof(string) && key.Equals(KeyedServices.Username, StringComparison.OrdinalIgnoreCase))
        {
            typedValidator = (IValidator<T>)this;
            return true;
        }

        return false;
    }
}