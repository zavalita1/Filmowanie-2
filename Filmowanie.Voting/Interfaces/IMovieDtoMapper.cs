using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.DTOs.Outgoing;

namespace Filmowanie.Voting.Interfaces;

internal interface IMovieDtoMapper
{
    Maybe<MovieDTO[]> Map(Maybe<(IReadOnlyMovieEntity[] MoviesEntities, IReadOnlyEmbeddedMovieWithVotes[] EmbeddedMovies, DomainUser CurrentUser)> input);
}