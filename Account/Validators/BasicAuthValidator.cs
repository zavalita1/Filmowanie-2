using Filmowanie.Account.Constants;
using Filmowanie.DTOs.Incoming;
using Filmowanie.Interfaces;
using FluentValidation;

namespace Filmowanie.Account.Validators;

public class BasicAuthValidator : AbstractValidator<BasicAuthLoginDto>, IFluentValidatorAdapter
{
    public BasicAuthValidator()
    {
        RuleFor(x => x.Email).NotNull().WithMessage($"{nameof(BasicAuthLoginDto.Email)} cannot be null!");
        RuleFor(x => x.Email).NotEmpty().WithMessage($"{nameof(BasicAuthLoginDto.Email)} cannot be empty!");
        RuleFor(x => x.Email).EmailAddress().WithMessage($"{nameof(BasicAuthLoginDto.Email)} must be a valid mail address!");
        RuleFor(x => x.Password).NotNull().WithMessage($"{nameof(BasicAuthLoginDto.Email)} cannot be null!");
        RuleFor(x => x.Password).NotEmpty().WithMessage($"{nameof(BasicAuthLoginDto.Email)} cannot be empty!");
    }

    public virtual bool CanHandle<T>(string key, out IValidator<T> typedValidator)
    {
        var result = typeof(T) == typeof(BasicAuthLoginDto) && key.Equals(KeyedServices.LoginViaBasicAuthKey);
        typedValidator = result ? (IValidator<T>) this : null;

        return result;
    }
}