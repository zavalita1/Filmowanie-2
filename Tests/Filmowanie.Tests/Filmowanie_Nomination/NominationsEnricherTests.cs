using Filmowanie.Abstractions.Extensions;
using FluentAssertions;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Nomination.DTOs.Outgoing;
using Filmowanie.Nomination.Mappers;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Filmowanie.Tests.Filmowanie_Nomination;

public sealed class NominationsEnricherTests
{
    private readonly NominationsEnricher _sut;
    private readonly IMovieDomainRepository _movieRepository;

    public NominationsEnricherTests()
    {
        var logger = Substitute.For<ILogger<NominationsEnricher>>();
        _movieRepository = Substitute.For<IMovieDomainRepository>();
        _sut = new NominationsEnricher(logger, _movieRepository);
    }

    [Fact]
    public async Task EnrichNominationsAsync_WhenNoMoviesCanBeNominatedAgain_ShouldReturnEmptyMoviesList()
    {
        // Arrange
        var input = new NominationsDataDTO { Nominations = ["020s", "010s"] };
        var maybe = input.AsMaybe();

        _movieRepository.GetMoviesThatCanBeNominatedAgainEventsAsync(Arg.Any<CancellationToken>())
            .Returns([]);
        _movieRepository.GetMovieNominatedEventsAsync(Arg.Any<CancellationToken>())
            .Returns([]);
        _movieRepository.GetManyByIdAsync(null!, CancellationToken.None).ReturnsForAnyArgs(new Maybe<IReadOnlyMovieEntity[]>([], null));

        // Act
        var result = await _sut.EnrichNominationsAsync(maybe, CancellationToken.None);

        // Assert
        result.Result.Should().NotBeNull();
        result.Result!.MoviesThatCanBeNominatedAgain.Should().BeEmpty();
        result.Result!.Nominations.Should().BeEquivalentTo(input.Nominations);
    }

    [Fact]
    public async Task EnrichNominationsAsync_WhenMoviesCanBeNominatedAgain_ShouldReturnFilteredMovies()
    {
        // Arrange
        var input = new NominationsDataDTO { Nominations = ["2020s", "2010s"] };
        var maybe = input.AsMaybe();

        var movieId1 = Guid.NewGuid();
        var movieId2 = Guid.NewGuid();
        var movieId3 = Guid.NewGuid();

        IReadOnlyCanNominateMovieAgainEvent[] canBeNominatedEvents =
        [
            CreateMovieCanBeNominatedAgainEvent(movieId1, 2023, DateTime.UtcNow),
            CreateMovieCanBeNominatedAgainEvent(movieId2, 2015, DateTime.UtcNow),
            CreateMovieCanBeNominatedAgainEvent(movieId3, 1995, DateTime.UtcNow)
        ];

        IReadOnlyNominatedMovieEvent[] nominatedEvents =
        [
            CreateMovieNominatedEvent(movieId1, 2023, DateTime.UtcNow.AddDays(-1)),
        ];

        IReadOnlyMovieEntity[] movies =
        [
            CreateMovie(movieId1, "Movie 1", 2023),
            CreateMovie(movieId2, "Movie 2", 2015)
        ];

        _movieRepository.GetMoviesThatCanBeNominatedAgainEventsAsync(Arg.Any<CancellationToken>())
            .Returns(canBeNominatedEvents);
        _movieRepository.GetMovieNominatedEventsAsync(Arg.Any<CancellationToken>())
            .Returns(nominatedEvents);
        _movieRepository.GetManyByIdAsync(Arg.Is<IEnumerable<string>>(y => y.Count() == 2 && y.Contains(movieId1.ToString()) && y.Contains(movieId2.ToString())), Arg.Any<CancellationToken>(), Arg.Any<bool>())
            .Returns(new Maybe<IReadOnlyMovieEntity[]>(movies, null));

        // Act
        var result = await _sut.EnrichNominationsAsync(maybe, CancellationToken.None);

        // Assert
        result.Result.Should().NotBeNull();
        result.Result!.Nominations.Should().BeEquivalentTo(input.Nominations);
        result.Result!.MoviesThatCanBeNominatedAgain.Should().HaveCount(2);
    }

    [Fact]
    public async Task EnrichNominationsAsync_WhenMovieIsNominatedAfterCanBeNominated_ShouldNotIncludeMovie()
    {
        // Arrange
        var input = new NominationsDataDTO { Nominations = ["2020s"] };
        var maybe = input.AsMaybe();

        var movieId = Guid.NewGuid();
        var movieId2 = Guid.NewGuid();
        var movieId3 = Guid.NewGuid();

        IReadOnlyCanNominateMovieAgainEvent[] canBeNominatedEvents =
        [
            CreateMovieCanBeNominatedAgainEvent(movieId, 2023, DateTime.UtcNow),
            CreateMovieCanBeNominatedAgainEvent(movieId2, 2023, DateTime.UtcNow),
            CreateMovieCanBeNominatedAgainEvent(movieId2, 2023, DateTime.UtcNow.AddDays(-10)),
            CreateMovieCanBeNominatedAgainEvent(movieId3, 2023, DateTime.UtcNow),
            CreateMovieCanBeNominatedAgainEvent(movieId3, 2023, DateTime.UtcNow.AddDays(-10)),
        ];

        IReadOnlyNominatedMovieEvent[] nominatedEvents =
        [
            CreateMovieNominatedEvent(movieId, 2023, DateTime.UtcNow),
            CreateMovieNominatedEvent(movieId2, 2023, DateTime.UtcNow.AddDays(-1)),
            CreateMovieNominatedEvent(movieId3, 2023, DateTime.UtcNow.AddDays(-1)),
            CreateMovieNominatedEvent(movieId3, 2023, DateTime.UtcNow),
        ];

        _movieRepository.GetMoviesThatCanBeNominatedAgainEventsAsync(Arg.Any<CancellationToken>())
            .Returns(canBeNominatedEvents);
        _movieRepository.GetMovieNominatedEventsAsync(Arg.Any<CancellationToken>())
            .Returns(nominatedEvents);
        _movieRepository.GetManyByIdAsync(null!, CancellationToken.None).ReturnsForAnyArgs(new Maybe<IReadOnlyMovieEntity[]>([], null));

        // Act
        var result = await _sut.EnrichNominationsAsync(maybe, CancellationToken.None);

        // Assert
        result.Result.Should().NotBeNull();
        result.Result!.MoviesThatCanBeNominatedAgain.Should().BeEmpty();
        await _movieRepository.Received(1).GetManyByIdAsync(Arg.Is<IEnumerable<string>>(y => y.Single() == movieId2.ToString()), CancellationToken.None, false);
    }

    private CanNominateMovieAgainEvent CreateMovieCanBeNominatedAgainEvent(Guid movieId, int year, DateTime created)
    {
        var movie = CreateEmbeddedMovie(movieId, "Test Movie", year);
        return new CanNominateMovieAgainEvent { Movie = movie, Created = created};
    }

    private NominatedMovieEvent CreateMovieNominatedEvent(Guid movieId, int year, DateTime created)
    {
        var movie = CreateEmbeddedMovie(movieId, "Test Movie", year);
        return new NominatedMovieEvent { Movie = movie, Created = created };
    }

    private EmbeddedMovie CreateEmbeddedMovie(Guid id, string name, int year)
    {
        return new EmbeddedMovie { id = id.ToString(), Name = name, MovieCreationYear = year };
    }

    private static MovieEntity CreateMovie(Guid id, string name, int year)
    {
        return new MovieEntity {
            id = id.ToString(),
            Name = name,
            PosterUrl = "http://example.com/poster.jpg",
            BigPosterUrl = "http://example.com/bigposter.jpg",
            Description = "Description",
            FilmwebUrl = "http://filmweb.com/movie",
            CreationYear = year,
            DurationInMinutes = 120,
            Genres = ["Action", "Drama"],
            Actors = ["Actor 1", "Actor 2"],
            Directors = ["Director 1"],
            Writers = ["Writer 1"],
            OriginalTitle = "Original Title"
        };
    }
}
