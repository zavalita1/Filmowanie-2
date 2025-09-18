using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Nomination.Models;
using FluentValidation;

namespace Filmowanie.Nomination.Validators;

// TODO UTs
internal class NominationMovieValidator : AbstractValidator<(IReadOnlyMovieEntity Movie, DomainUser User, CurrentNominationsData CurrentNominations)>, IFluentValidatorAdapter
{
    public NominationMovieValidator()
    {
        RuleFor(x => x.Movie.Genres).Must(x => !x.Contains("horror", StringComparer.OrdinalIgnoreCase)).WithMessage("Horrors are not allowed. Nice try motherfucker.");
        RuleFor(x => x).Must(x => x.CurrentNominations.NominationData.Where(y => x.User.Id == y.User!.Id).Select(y => y.Year).Any(y => x.Movie.CreationYear.ToDecade() == y))
            .WithMessage("This movie is not made during a decade you're allowed to nominate from");
    }

    public bool CanHandle<T>(string key, out IValidator<T>? typedValidator)
    {
        typedValidator = null;
        if (typeof(T) == typeof((IReadOnlyMovieEntity, DomainUser, CurrentNominationsData)))
        {
            typedValidator = (IValidator<T>)this;
            return true;
        }

        return false;
    }
}