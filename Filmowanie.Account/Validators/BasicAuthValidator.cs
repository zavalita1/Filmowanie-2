using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Account.Constants;
using Filmowanie.Account.DTOs.Incoming;
using FluentValidation;

namespace Filmowanie.Account.Validators;

public class BasicAuthValidator : AbstractValidator<BasicAuthLoginDTO>, IFluentValidatorAdapter
{
    public BasicAuthValidator()
    {
        RuleFor(x => x.Email).NotNull().WithMessage($"{nameof(BasicAuthLoginDTO.Email)} cannot be null!");
        RuleFor(x => x.Email).NotEmpty().WithMessage($"{nameof(BasicAuthLoginDTO.Email)} cannot be empty!");
        RuleFor(x => x.Email).EmailAddress().WithMessage($"{nameof(BasicAuthLoginDTO.Email)} must be a valid mail address!");
        RuleFor(x => x.Password).NotNull().WithMessage($"{nameof(BasicAuthLoginDTO.Email)} cannot be null!");
        RuleFor(x => x.Password).NotEmpty().WithMessage($"{nameof(BasicAuthLoginDTO.Email)} cannot be empty!");
    }

    public virtual bool CanHandle<T>(string key, out IValidator<T>? typedValidator)
    {
        var result = typeof(T) == typeof(BasicAuthLoginDTO) && key.Equals(KeyedServices.LoginViaBasicAuthKey);
        typedValidator = result ? (IValidator<T>) this : null;

        return result;
    }
}