using AutoFixture;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Nomination.Builders;
using FluentAssertions;
using NSubstitute;
using Microsoft.Extensions.Logging;
using Filmowanie.Abstractions.Configuration;
using Microsoft.Extensions.Options;

namespace Filmowanie.UnitTests.Filmowanie_Nomination;

public class MovieBuilderTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void Build_ShouldThrowArgumentException_WhenRequiredFieldsAreMissing()
    {
        // Arrange
        var guidProvider = Substitute.For<IGuidProvider>();
        var dateTimeProvider = Substitute.For<IDateTimeProvider>();
        var log = Substitute.For<ILogger>();
        var options = Substitute.For<IOptions<FilmwebOptions>>();
        var builder = new MovieBuilder(options, log);

        // Act
        Action act = () => builder.Build(guidProvider, dateTimeProvider);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Cannot construct this!");
    }

    [Fact]
    public void Build_ShouldReturnMovie_WhenAllRequiredFieldsAreProvided()
    {
        // Arrange
        var guidProvider = Substitute.For<IGuidProvider>();
        var dateTimeProvider = Substitute.For<IDateTimeProvider>();

        var guid = _fixture.Create<Guid>();
        guidProvider.NewGuid().Returns(guid);

        var now = _fixture.Create<DateTime>();
        dateTimeProvider.Now.Returns(now);
        var log = Substitute.For<ILogger>();

        var options = Substitute.For<IOptions<FilmwebOptions>>();

        var builder = new MovieBuilder(options, log)
            .WithName("Inception")
            .WithTenant(new TenantId(1))
            .WithOriginalTitle("Inception")
            .WithPosterUrl("http://example.com/poster.jpg")
            .WithBigPosterUrl("http://example.com/bigposter.jpg")
            .WithDescription("Dream within a dream within a dream I suppose")
            .WithCreationYear("2010")
            .WithDuration("148")
            .WithGenre("Sci-Fi")
            .WithActor("Leonard DiCaprio")
            .WithDirector("Krzysiu Nolan")
            .WithWriter("Krzysiu Nolan")
            .WithFilmwebUrl("http://example.com/inception");

        // Act
        var movie = builder.Build(guidProvider, dateTimeProvider);

        // Assert
        movie.Name.Should().Be("Inception");
        movie.OriginalTitle.Should().Be("Inception");
        movie.Description.Should().Be("Dream within a dream within a dream I suppose");
        movie.PosterUrl.Should().Be("http://example.com/poster.jpg");
        movie.BigPosterUrl.Should().Be("http://example.com/bigposter.jpg");
        movie.FilmwebUrl.Should().Be("http://example.com/inception");
        movie.Actors.Should().Contain("Leonard DiCaprio");
        movie.Directors.Should().Contain("Krzysiu Nolan");
        movie.Writers.Should().Contain("Krzysiu Nolan");
        movie.Genres.Should().Contain("Sci-Fi");
        movie.CreationYear.Should().Be(2010);
        movie.DurationInMinutes.Should().Be(148);
        movie.TenantId.Should().Be(1);
        movie.id.Should().Be("movie-"+guid);
        movie.Created.Should().Be(now);
    }
}
