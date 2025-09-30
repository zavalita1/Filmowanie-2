using Filmowanie.Nomination.Consts;
using FluentAssertions;
using NSubstitute;
using System.Net;
using Filmowanie.Nomination.Services;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;
using Filmowanie.Abstractions.Configuration;
using Microsoft.Extensions.Options;

namespace Filmowanie.UnitTests.Filmowanie_Nomination;

public sealed class FilmwebPostersUrlsRetrieverTests
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly FilmwebPostersUrlsRetriever _retriever;

    public FilmwebPostersUrlsRetrieverTests()
    {
        _clientFactory = Substitute.For<IHttpClientFactory>();
        var options = Substitute.For<IOptions<FilmwebOptions>>();
        options.Value.Returns(new FilmwebOptions { BaseUrl = "https://www.filmweb.pl/", FilmApiUrl = "https://www.filmweb.pl/film/", FallbackBigPosterUrl = "", FallbackPosterUrl = "" });
        _retriever = new FilmwebPostersUrlsRetriever(_clientFactory, options);
    }

    [Fact]
    public async Task GetPosterUrlsAsync_ShouldReturnUrls_WhenResponseIsSuccessful()
    {
        // Arrange
        var metadata = new FilmwebUriMetadata("film/Some-Movie-2023-123456", "https://www.filmweb.pl/film/Some-Movie-2023-123456", 123456);
        var httpClient = Substitute.For<HttpClient>();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(@"<html><div>some bs tags and whatnot</div><img src=""https://fwcdn.pl/fpo/123456/123456.10.jpg"" /></html>")
        };
        httpClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(responseMessage));
        _clientFactory.CreateClient(HttpClientNames.Filmweb).Returns(httpClient);

        // Act
        var result = await _retriever.GetPosterUrlsAsync(metadata, CancellationToken.None);

        // Assert
        result.Should().ContainSingle(url => url == "https://fwcdn.pl/fpo/123456/123456.10.jpg");
    }

    [Fact]
    public async Task GetPosterUrlsAsync_ShouldThrowValidationException_WhenResponseIsUnsuccessful()
    {
        // Arrange
        var metadata = new FilmwebUriMetadata("film/Some-Movie-2023-123456", "https://www.filmweb.pl/film/Some-Movie-2023-123456", 123456);
        var httpClient = Substitute.For<HttpClient>();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);
        httpClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(responseMessage));
        _clientFactory.CreateClient(HttpClientNames.Filmweb).Returns(httpClient);

        // Act
        Func<Task> act = async () => await _retriever.GetPosterUrlsAsync(metadata, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>().WithMessage("Cannot access filmweb. Check if the link provided is correct. If it is, try again. If this issue persists, contact admin.");
    }
}
