using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Account.DTOs.Incoming;
using FluentValidation;

namespace Filmowanie.Account.Validators;

// TODO UTs
internal class GoogleOAuthClientDTOValidator : AbstractValidator<GoogleOAuthClientDTO>, IFluentValidatorAdapter
{
    public GoogleOAuthClientDTOValidator()
    {
        RuleFor(x => x.Code).NotNull().WithMessage($"{nameof(GoogleOAuthClientDTO.Code)} cannot be null!");
        RuleFor(x => x.Scope).NotNull().WithMessage($"{nameof(GoogleOAuthClientDTO.Scope)} cannot be null!");
    }

    public bool CanHandle<T>(string key, out IValidator<T> typedValidator)
    {
        var result = typeof(T) == typeof(GoogleOAuthClientDTO);
        typedValidator = result ? (IValidator<T>)this : null!;

        return result;
    }
}