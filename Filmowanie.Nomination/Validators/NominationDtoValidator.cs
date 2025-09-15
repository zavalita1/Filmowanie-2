using Filmowanie.Abstractions;
using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Nomination.DTOs.Incoming;
using Filmowanie.Nomination.Models;
using FluentValidation;

namespace Filmowanie.Nomination.Validators;

// TODO UTs
internal class NominationDtoValidator : AbstractValidator<(NominationDTO Dto, DomainUser User, CurrentNominationsData CurrentNominations)>, IFluentValidatorAdapter
{
    public NominationDtoValidator()
    {
        RuleFor(x => x.Dto).NotNull().WithMessage("Value cannot be null!");
        RuleFor(x => x.Dto).NotNull().WithMessage($"{nameof(NominationDTO.PosterUrl)} cannot be null!");
        RuleFor(x => x.Dto).NotEmpty().WithMessage($"{nameof(NominationDTO.PosterUrl)} cannot be empty!");
        RuleFor(x => x.Dto.PosterUrl).Must(x => x!.StartsWith("http")).WithMessage($"{nameof(NominationDTO.PosterUrl)} must be a valid http link!").When(x => !string.IsNullOrEmpty(x.Dto.PosterUrl));
        RuleFor(x => x.Dto.MovieFilmwebUrl).SetValidator(new NominationMovieUrlValidator()).DependentRules(() =>
        {
            RuleFor(x => x).Must(x =>
            {
                var movieYear = NominationMovieUrlValidator.GetGeneratedRegex().Match(x.Dto.MovieFilmwebUrl).Groups[1];
                var decade = int.Parse(movieYear.Value).ToDecade();
                return x.CurrentNominations.NominationData.Any(y => y.User!.Id == x.User.Id && y.Year == decade);
            }).WithMessage($"User must have nominations from proper decade to assign!");
        });

        RuleFor(x => x).Must(x => x.CurrentNominations.NominationData.Any(y => y.User!.Id == x.User.Id)).WithMessage("User must have nominations to assign!");
    }

    public bool CanHandle<T>(string key, out IValidator<T>? typedValidator)
    {
        typedValidator = null;
        if (typeof(T) == typeof((NominationDTO, DomainUser, CurrentNominationsData)))
        {
            typedValidator = (IValidator<T>)this;
            return true;
        }

        return false;
    }
}