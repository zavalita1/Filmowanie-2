using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Voting.DTOs.Outgoing;
using Filmowanie.Voting.Interfaces;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Voting.Mappers;

// TODO UTs
internal sealed class MovieDtoMapper : IMovieDtoMapper
{
    private readonly ILogger<MovieDtoMapper> log;

    public MovieDtoMapper(ILogger<MovieDtoMapper> log)
    {
        this.log = log;
    }

    public Maybe<MovieDTO[]> Map(Maybe<(IReadOnlyMovieEntity[] MoviesEntities, IReadOnlyEmbeddedMovieWithVotes[] EmbeddedMovies, DomainUser CurrentUser)> input) => input.Accept(Map, this.log);

    private Maybe<MovieDTO[]> Map((IReadOnlyMovieEntity[] MoviesEntities, IReadOnlyEmbeddedMovieWithVotes[] EmbeddedMovies, DomainUser CurrentUser) input)
    {
        var movies = input.EmbeddedMovies.Join(input.MoviesEntities, x => x.Movie.id, x => x.id, (x, y) => new { Movie = y, x.Votes });

        var resultMovies = new List<MovieDTO>(input.EmbeddedMovies.Length);

        foreach (var movie in movies)
        {
            log.LogInformation($"Mapping movie: {movie.Movie.Name}");
            var votes = (int?)movie.Votes.SingleOrDefault(x => x.User.id == input.CurrentUser.Id)?.VoteType ?? 0;
            var duration = movie.Movie.DurationInMinutes.GetDurationString();
            var movieDto = new MovieDTO(movie.Movie.id, movie.Movie.Name, votes, movie.Movie.PosterUrl, movie.Movie.BigPosterUrl, movie.Movie.Description, movie.Movie.AltDescription, movie.Movie.FilmwebUrl, movie.Movie.CreationYear, duration, movie.Movie.Genres, movie.Movie.Actors,
                movie.Movie.Directors, movie.Movie.Writers, movie.Movie.OriginalTitle);

            resultMovies.Add(movieDto);
        }

        return resultMovies.ToArray().AsMaybe();
    }
}