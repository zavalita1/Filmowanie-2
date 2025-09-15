using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Nomination.Consts;
using FluentValidation;

namespace Filmowanie.Nomination.Validators;

// TODO UTs
internal class NominationMovieIdValidator : AbstractValidator<string>, IFluentValidatorAdapter
{
    private const string MovieIdPrefix = "movie-";

    public NominationMovieIdValidator()
    {
        RuleFor(x => x).NotNull().WithMessage("Value cannot be null!");
        RuleFor(x => x).Must(x => x.StartsWith(MovieIdPrefix)).WithMessage($"Value must start with prefix {MovieIdPrefix}!");
        RuleFor(x => x).Must(x => Guid.TryParse((string?)x[MovieIdPrefix.Length..], out _)).WithMessage($"Value must be a valid guid!");
    }

    public bool CanHandle<T>(string key, out IValidator<T>? typedValidator)
    {
        typedValidator = null;
        if (typeof(T) == typeof(string) && key.Equals(KeyedServices.MovieId, StringComparison.OrdinalIgnoreCase))
        {
            typedValidator = (IValidator<T>)this;
            return true;
        }

        return false;
    }
}