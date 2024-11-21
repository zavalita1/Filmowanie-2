using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Account.Constants;
using FluentValidation;

namespace Filmowanie.Account.Validators;

public class UserIdValidator : AbstractValidator<string>, IFluentValidatorAdapter
{
    private readonly char[] _legalNonAlphanumericChars = [' ', '/', '"', '\'', '/', '_'];

    public UserIdValidator()
    {
        RuleFor(x => x).NotNull().WithMessage("Value cannot be null!");
        RuleFor(x => x).NotEmpty().WithMessage("Value cannot be empty!");
        RuleFor(x => x).Length(6, 30).WithMessage("Length must be between 6 and 30 characters");
        RuleFor(x => x).Must(x => x.All(y => char.IsLetterOrDigit(y) || _legalNonAlphanumericChars.Contains(y))).WithMessage("Can't contain illegal characters");
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