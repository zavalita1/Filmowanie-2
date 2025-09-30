using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Filmowanie.Abstractions.Configuration;
using Filmowanie.Nomination.Consts;
using Filmowanie.Nomination.Interfaces;
using Microsoft.Extensions.Options;

namespace Filmowanie.Nomination.Services;

internal sealed class FilmwebPostersUrlsRetriever : IFilmwebPostersUrlsRetriever
{
    private readonly IHttpClientFactory clientFactory;
    private readonly IOptions<FilmwebOptions> options;

    private static string GetPostersPattern(int movieId) => $@"src=""(https:\/\/fwcdn\.pl.*?\/{movieId}\/.*?\.10\..*?)""";

    public FilmwebPostersUrlsRetriever(IHttpClientFactory clientFactory, IOptions<FilmwebOptions> options)
    {
        this.clientFactory = clientFactory;
        this.options = options;
    }

    public async Task<IEnumerable<string>> GetPosterUrlsAsync(FilmwebUriMetadata metadata, CancellationToken cancel)
    {
        var client = this.clientFactory.CreateClient(HttpClientNames.Filmweb);
        client.BaseAddress = new Uri(this.options.Value.BaseUrl, UriKind.Absolute);
        var postersUrl = $"{metadata.MovieAbsolutePath}/posters";

        using var apiRequest = new HttpRequestMessage(HttpMethod.Get, postersUrl);
        apiRequest.Headers.Add("x-locale", "pl");

        var response = await client.SendAsync(apiRequest, cancel);

        if (!response.IsSuccessStatusCode)
            throw new ValidationException(
                "Cannot access filmweb. Check if the link provided is correct. If it is, try again. If this issue persists, contact admin.");

        var content = await response.Content.ReadAsStringAsync(cancel);
        var pattern = GetPostersPattern(metadata.MovieId);
        var matches = Regex.Matches(content, pattern);

        var result = new List<string>(matches.Count);
        foreach (Match match in matches)
        {
            result.Add(match.Groups[1].Value);
        }

        return result;
    }
}