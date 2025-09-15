using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Account.Constants;
using Filmowanie.Account.DTOs.Incoming;
using FluentValidation;

namespace Filmowanie.Account.Validators;

internal class UserDTOValidator : AbstractValidator<UserDTO>, IFluentValidatorAdapter
{
    public UserDTOValidator()
    {
        RuleFor(x => x.Id).NotNull().WithMessage("Value cannot be null!");
        RuleFor(x => x.Id).SetValidator(new UserIdValidator());
    }

    public bool CanHandle<T>(string key, out IValidator<T>? typedValidator)
    {
        typedValidator = null;
        if (typeof(T) == typeof(UserDTO) && key.Equals(KeyedServices.Username, StringComparison.OrdinalIgnoreCase))
        {
            typedValidator = (IValidator<T>)this;
            return true;
        }

        return false;
    }
}