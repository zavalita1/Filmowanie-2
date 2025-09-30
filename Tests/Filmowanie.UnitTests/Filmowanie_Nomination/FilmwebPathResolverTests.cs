using FluentAssertions;
using Filmowanie.Nomination.Services;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;
using NSubstitute;
using MassTransit.Configuration;
using Filmowanie.Abstractions.Configuration;
using Microsoft.Extensions.Options;

namespace Filmowanie.UnitTests.Filmowanie_Nomination;

public class FilmwebPathResolverTests
{
    private readonly FilmwebPathResolver _resolver;

    public FilmwebPathResolverTests()
    {
        var options = Substitute.For<IOptions<FilmwebOptions>>();
        _resolver = new FilmwebPathResolver(options);
    }

    [Fact]
    public void GetMetadata_ShouldReturnCorrectMetadata_WhenPathIsValid()
    {
        // Arrange
        var filmwebMovieAbsolutePath = "https://www.filmweb.pl/film/Some-Movie-2023-123456";

        // Act
        var metadata = _resolver.GetMetadata(filmwebMovieAbsolutePath);

        // Assert
        metadata.MovieRelativePath.Should().Be("film/Some-Movie-2023-123456");
        metadata.MovieAbsolutePath.Should().Be("https://www.filmweb.pl/film/Some-Movie-2023-123456");
        metadata.MovieId.Should().Be(123456);
    }

    [Fact]
    public void GetMetadata_ShouldThrowValidationException_WhenPathIsInvalid()
    {
        // Arrange
        const string invalidPath = "https://www.filmweb.pl/invalid-path";

        // Act
        Action act = () => _resolver.GetMetadata(invalidPath);

        // Assert
        act.Should().Throw<ValidationException>().WithMessage("Cannot get film id!");
    }
}