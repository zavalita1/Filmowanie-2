using System.Text.RegularExpressions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Nomination.Consts;
using Filmowanie.Nomination.DTOs.Incoming;
using FluentValidation;

namespace Filmowanie.Nomination.Validators;

// TODO UTs
internal partial class NominationMovieUrlValidator : AbstractValidator<string>, IFluentValidatorAdapter
{
    [GeneratedRegex("https://www\\.filmweb\\.pl/film/[^-]*-([0-9][0-9][0-9][0-9])-", RegexOptions.IgnoreCase)]
    internal static partial Regex GetGeneratedRegex();

    public NominationMovieUrlValidator()
    {
        RuleFor(x => x).NotNull().WithMessage("Value cannot be null!");
        RuleFor(x => x).NotEmpty().WithMessage($"{nameof(NominationDTO.MovieFilmwebUrl)} cannot be empty!");
        RuleFor(x => x).Must(x => GetGeneratedRegex().Match(x).Success).WithMessage($"{nameof(NominationDTO.MovieFilmwebUrl)} must be a valid filmweb link!");
    }

    public bool CanHandle<T>(string key, out IValidator<T>? typedValidator)
    {
        typedValidator = null;
        if (typeof(T) == typeof(string) && key.Equals(KeyedServices.MovieUrl, StringComparison.OrdinalIgnoreCase))
        {
            typedValidator = (IValidator<T>)this;
            return true;
        }

        return false;
    }
}