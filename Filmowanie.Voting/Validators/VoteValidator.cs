using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Voting.DTOs.Incoming;
using FluentValidation;

namespace Filmowanie.Voting.Validators;

internal class VoteValidator : AbstractValidator<VoteDTO>, IFluentValidatorAdapter
{
    private const string MovieIdPrefix = "movie-";

    public VoteValidator()
    {
        RuleFor(x => x).NotNull().WithMessage("Value cannot be null!");
        RuleFor(x => x.Votes).GreaterThanOrEqualTo(-1).WithMessage("Votes must be greater than -2!");
        RuleFor(x => x.MovieTitle).NotNull().WithMessage($"{nameof(VoteDTO.MovieTitle)} must not be null!");
        RuleFor(x => x.MovieTitle).NotEmpty().WithMessage($"{nameof(VoteDTO.MovieTitle)} must not be empty!");
        RuleFor(x => x.MovieId).NotNull().WithMessage($"{nameof(VoteDTO.MovieTitle)} must not be null!");
        RuleFor(x => x.MovieId).NotEmpty().WithMessage($"{nameof(VoteDTO.MovieTitle)} must not be empty!");
        RuleFor(x => x.MovieId).Must(x => x.StartsWith(MovieIdPrefix)).WithMessage($"{nameof(VoteDTO.MovieId)} must start with prefix {MovieIdPrefix}!").When(x => !string.IsNullOrEmpty(x.MovieId));
        RuleFor(x => x.MovieId).Must(x => Guid.TryParse((string?)x[MovieIdPrefix.Length..], out _)).WithMessage($"{nameof(VoteDTO.MovieId)} must be a valid guid!").When(x => !string.IsNullOrEmpty(x.MovieId));
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