using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Voting.DTOs.Incoming;
using FluentValidation;

namespace Filmowanie.Voting.Validators;

internal class VoteValidator : AbstractValidator<VoteDTO>, IFluentValidatorAdapter
{
    public VoteValidator()
    {
        RuleFor(x => x).NotNull().WithMessage("Value cannot be null!");
        RuleFor(x => x.Votes).GreaterThanOrEqualTo(0).WithMessage("Votes must be nonnegative!");
        RuleFor(x => x.MovieTitle).NotNull().WithMessage($"{nameof(VoteDTO.MovieTitle)} must not be null!");
        RuleFor(x => x.MovieTitle).NotEmpty().WithMessage($"{nameof(VoteDTO.MovieTitle)} must not be empty!");
        RuleFor(x => x.MovieId).NotEmpty().WithMessage($"{nameof(VoteDTO.MovieTitle)} must not be empty!");
        RuleFor(x => x.MovieId).Must(x => Guid.TryParse((string?)x, out _)).WithMessage($"{nameof(VoteDTO.MovieId)} must be a valid guid!");
    }

    public bool CanHandle<T>(string key, out IValidator<T>? typedValidator)
    {
        typedValidator = null;
        if (typeof(T) == typeof(VoteDTO))
        {
            typedValidator = (IValidator<T>)this;
            return true;
        }

        return false;
    }
}