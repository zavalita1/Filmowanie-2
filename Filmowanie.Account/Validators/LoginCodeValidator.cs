using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Account.Constants;
using Filmowanie.Account.DTOs.Incoming;
using FluentValidation;

namespace Filmowanie.Account.Validators;

public class LoginCodeValidator : AbstractValidator<LoginDto>, IFluentValidatorAdapter
{
    public LoginCodeValidator()
    {
        RuleFor(x => x.Code).NotNull().WithMessage($"{nameof(LoginDto.Code)} cannot be null!");
        RuleFor(x => x.Code).NotEmpty().WithMessage($"{nameof(LoginDto.Code)} cannot be empty!");
        RuleFor(x => x.Code).Must(x => Guid.TryParse((string?)x, out _)).WithMessage($"{nameof(LoginDto.Code)} must be a valid guid!");
    }

    public bool CanHandle<T>(string key, out IValidator<T> typedValidator)
    {
        var result = typeof(T) == typeof(LoginDto);
        typedValidator = result ? (IValidator<T>) this : null;

        return result;
    }
}