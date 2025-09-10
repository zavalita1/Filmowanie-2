using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Nomination.DTOs.Incoming;

namespace Filmowanie.Nomination.Interfaces;

internal interface IFilmwebHandler
{
    Task<Maybe<IReadOnlyMovieEntity>> GetMovieAsync(Maybe<(NominationDTO NominationDto, DomainUser CurrentUser)> input, CancellationToken cancel);
}