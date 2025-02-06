using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Notification.DTOs.Incoming;
using FluentValidation;

namespace Filmowanie.Notification.Validators;

internal class PushSubscriptionDTOValidator : AbstractValidator<PushSubscriptionDTO>, IFluentValidatorAdapter
{
    public PushSubscriptionDTOValidator()
    {
        RuleFor(x => x).NotNull().WithMessage("Value cannot be null!");
        RuleFor(x => x.Endpoint).NotEmpty().WithMessage($"{nameof(PushSubscriptionDTO.Endpoint)} cannot be empty!");
        RuleFor(x => x.Auth).NotEmpty().WithMessage($"{nameof(PushSubscriptionDTO.Auth)} cannot be empty!");
        RuleFor(x => x.Client).NotEmpty().WithMessage($"{nameof(PushSubscriptionDTO.Client)} cannot be empty!");
        RuleFor(x => x.P256DH).NotEmpty().WithMessage($"{nameof(PushSubscriptionDTO.P256DH)} cannot be empty!");
    }

    public bool CanHandle<T>(string key, out IValidator<T>? typedValidator)
    {
        typedValidator = null;
        if (typeof(T) == typeof(PushSubscriptionDTO))
        {
            typedValidator = (IValidator<T>)this;
            return true;
        }

        return false;
    }
}