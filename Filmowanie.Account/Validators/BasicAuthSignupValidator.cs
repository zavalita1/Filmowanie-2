using Filmowanie.Account.Constants;
using Filmowanie.DTOs.Incoming;
using FluentValidation;

namespace Filmowanie.Account.Validators;

public class BasicAuthSignupValidator : BasicAuthValidator
{
    private const int MinimalPasswordLength = 7;

    public BasicAuthSignupValidator()
    {
        RuleFor(x => x.Password).Must(x => x.Length >= MinimalPasswordLength).WithMessage($"{nameof(BasicAuthLoginDto.Password)} must have {MinimalPasswordLength} characters or more");
    }

    public override bool CanHandle<T>(string key, out IValidator<T> typedValidator)
    {
        var result = typeof(T) == typeof(BasicAuthLoginDto) && key.Equals(KeyedServices.SignUpBasicAuth);
        typedValidator = result ? (IValidator<T>)this : null;

        return result;
    }
}