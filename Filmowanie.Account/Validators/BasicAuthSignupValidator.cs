using Filmowanie.Account.Constants;
using Filmowanie.Account.DTOs.Incoming;
using FluentValidation;

namespace Filmowanie.Account.Validators;

internal class BasicAuthSignupValidator : BasicAuthValidator
{
    private const int MinimalPasswordLength = 7;

    public BasicAuthSignupValidator()
    {
        RuleFor(x => x.Password).Must(x => x.Length >= MinimalPasswordLength).WithMessage($"{nameof(BasicAuthLoginDTO.Password)} must have {MinimalPasswordLength} characters or more");
    }

    public override bool CanHandle<T>(string key, out IValidator<T>? typedValidator)
    {
        var result = typeof(T) == typeof(BasicAuthLoginDTO) && key.Equals(KeyedServices.SignUpBasicAuth);
        typedValidator = result ? (IValidator<T>)this : null;

        return result;
    }
}