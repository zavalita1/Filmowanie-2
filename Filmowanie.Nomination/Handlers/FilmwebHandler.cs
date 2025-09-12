using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Web;
using Filmowanie.Abstractions;
using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Nomination.Builders;
using Filmowanie.Nomination.Consts;
using Filmowanie.Nomination.DTOs.Incoming;
using Filmowanie.Nomination.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace Filmowanie.Nomination.Handlers;

// TODO refactor to ditch regexes, write UTs
internal sealed partial class FilmwebHandler : IFilmwebHandler
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly IGuidProvider _guidProvider;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IFilmwebPathResolver _filmwebPathResolver;
    private ILogger<FilmwebHandler> _log;

    // TODO move away from regexes
    // TODO add validation against movie being in cinemas
    [GeneratedRegex(@"<meta property=\""og\:([A-z]*)\"" content=\""(.*?)\""", RegexOptions.IgnoreCase, 1000 * 60)]
    private static partial Regex GetMetaElementsRegexPatternGeneratedRegex();

    [GeneratedRegex(@"class=""filmCoverSection__([A-z]*)[A-z _-]*"">([A-z 0-9]*?)<\/", RegexOptions.IgnoreCase, 1000 * 60)]
    private static partial Regex GetTitleDetailsRegexPatternGeneratedRegex();

    [GeneratedRegex(@"data-duration=""([0-9]*)""", RegexOptions.IgnoreCase, 1000 * 60)]
    private static partial Regex GetDurationPatternRegex();

    [GeneratedRegex(@"descriptionSection__moreText.*?>(.*?)</", RegexOptions.IgnoreCase, 1000 * 60)]
    private static partial Regex GetLongDescriptionPatternRegex();

    [GeneratedRegex(@"href=""/person/[A-z\+ -%]*[0-9 -\.]*""> <span data-person-source>([A-ż -\.]*)</span>", RegexOptions.IgnoreCase, 1000 * 60)]
    private static partial Regex GetActorsPatternRegex();

    [GeneratedRegex(@"href=""/person/([A-z\+ -%0-9\.]*).*? itemprop=""director""", RegexOptions.IgnoreCase, 1000 * 60)]
    private static partial Regex GetDirectorsPatternRegex();

    [GeneratedRegex(@"href=""/person/([A-z\+ -%0-9\.]*).*? itemprop=""creator""", RegexOptions.IgnoreCase, 1000 * 60)]
    private static partial Regex GetWritersPatternRegex();

    private static string GetDataImagePattern(int movieId) => $@"data-image=""([^""]*?\/{movieId}\/.*?)""";


    public FilmwebHandler(IHttpClientFactory clientFactory, IGuidProvider guidProvider, IDateTimeProvider dateTimeProvider, IFilmwebPathResolver filmwebPathResolver, ILogger<FilmwebHandler> log)
    {
        _clientFactory = clientFactory;
        _guidProvider = guidProvider;
        _dateTimeProvider = dateTimeProvider;
        _filmwebPathResolver = filmwebPathResolver;
        _log = log;
    }

    public Task<Maybe<IReadOnlyMovieEntity>> GetMovieAsync(Maybe<(NominationDTO NominationDto, DomainUser CurrentUser)> input, CancellationToken cancel) =>
        input.AcceptAsync(GetMovieAsync, _log, cancel);


    public async Task<Maybe<IReadOnlyMovieEntity>> GetMovieAsync((NominationDTO NominationDto, DomainUser CurrentUser) input, CancellationToken cancel)
    {
        var metadata = _filmwebPathResolver.GetMetadata(input.NominationDto.MovieFilmwebUrl);
        var client = _clientFactory.CreateClient(HttpClientNames.Filmweb);
        var metadataRoute = $"{Urls.FilmwebApiUrl}{metadata.MovieId}/info";
        using var apiRequest = new HttpRequestMessage(HttpMethod.Get, metadataRoute);
        apiRequest.Headers.Add("x-locale", "pl");
        
        var tasks = new [] { 
            client.GetAsync(metadata.MovieRelativePath, cancel),
            client.SendAsync(apiRequest, cancel)};

        var responses = await Task.WhenAll(tasks);

        if (!responses[0].IsSuccessStatusCode || !responses[1].IsSuccessStatusCode)
            return new Error<IReadOnlyMovieEntity>("Cannot access filmweb. Check if the link provided is correct. If it is, try again. If this issue persists, contact admin.", ErrorType.Network);

        var responsesContentTask = responses[0].Content.ReadAsStringAsync(cancel);
        var responsesContent2Task = responses[1].Content.ReadFromJsonAsync<FilmwebInfoDTO>(cancellationToken: cancel);

        var responseContent = await responsesContentTask;
        var responseContent2 = await responsesContent2Task;

        var movieBuilder = new MovieBuilder().WithFilmwebUrl(metadata.MovieAbsolutePath);
        var web = new HtmlDocument();
        web.LoadHtml(responseContent);

        ExtractDataFromMetaElements(responseContent, ref movieBuilder);
        ExtractDataFromTitleDetailsElements(responseContent, ref movieBuilder);
        ExtractDataDuration(responseContent, ref movieBuilder);
        ExtractLongDescription(responseContent, ref movieBuilder);
        ExtractGenres(ref movieBuilder, web);
        ExtractActors(responseContent, ref movieBuilder, web);
        ExtractDirectors(responseContent, ref movieBuilder);
        ExtractWriters(responseContent, ref movieBuilder);

        if (!string.IsNullOrEmpty(responseContent2.Title))
            movieBuilder.WithName(responseContent2.Title);

        if (!string.IsNullOrEmpty(responseContent2.OriginalTitle))
            movieBuilder.WithOriginalTitle(responseContent2.OriginalTitle);

        if (responseContent2.Year != default)
            movieBuilder.WithCreationYear(responseContent2.Year.ToString());

        if (!string.IsNullOrEmpty(input.NominationDto.PosterUrl))
        {
            movieBuilder.WithPosterUrl(input.NominationDto.PosterUrl);
            movieBuilder.WithBigPosterUrl(GetPosterUrl(input.NominationDto.PosterUrl, true));
        }
        else
            ExtractDataImage(responseContent, metadata, ref movieBuilder);

        movieBuilder = movieBuilder.WithTenant(input.CurrentUser.Tenant);

        return movieBuilder.Build(_guidProvider, _dateTimeProvider).AsMaybe();
    }

    private static void ExtractDataImage(string responseContent, FilmwebUriMetadata metadata, ref MovieBuilder movieBuilder)
    {
        var pattern = GetDataImagePattern(metadata.MovieId);
        var match = Regex.Match(responseContent, pattern);

        if (match.Success)
        {
            var posterUrl = match.Groups[1].Value;
            posterUrl = GetPosterUrl(posterUrl, false);
            var bigPoster = GetPosterUrl(posterUrl, true);
            movieBuilder.WithPosterUrl(posterUrl);
            movieBuilder.WithBigPosterUrl(bigPoster);
        }
    }

    private static string GetPosterUrl(string posterUrl, bool isBigPoster)
    {
        var extensionIndex = posterUrl.LastIndexOf(".");
        var size = isBigPoster ? "8" : "10";
        posterUrl = posterUrl[..(extensionIndex - 1)] + size + posterUrl[extensionIndex..];
        return posterUrl;
    }

    private void ExtractDataFromMetaElements(string responseContent, ref MovieBuilder movieBuilder)
    {
        var matches = GetMetaElementsRegexPatternGeneratedRegex().Matches(responseContent);

        foreach (Match match in matches)
        {
            switch (match.Groups[1].Value)
            {
                case "description":
                    var description = match.Groups[2].Value;
                    description = HttpUtility.HtmlDecode(description);
                    movieBuilder.WithDescription(description);
                    break;
                case "image":
                    var posterUrl = match.Groups[2].Value;
                    var extensionIndex = posterUrl.LastIndexOf(".");
                    posterUrl = posterUrl[..(extensionIndex - 1)] + "6" + posterUrl[extensionIndex..];
                    movieBuilder.WithPosterUrl(posterUrl);
                    break;
                case "title":
                    var name = match.Groups[2].Value.Split("|").First().TrimEnd();
                    name = HttpUtility.HtmlDecode(name);
                    movieBuilder.WithName(name);
                    break;
                default:
                    break;
            }
        }
    }

    private static void ExtractDataFromTitleDetailsElements(string responseContent, ref MovieBuilder movieBuilder)
    {
        var matches = GetTitleDetailsRegexPatternGeneratedRegex().Matches(responseContent);

        foreach (Match match in matches)
        {
            switch (match.Groups[1].Value)
            {
                case "originalTitle":
                    var title = match.Groups[2].Value.Split("|").First().Trim();
                    movieBuilder.WithOriginalTitle(title);
                    break;
                case "year":
                    movieBuilder.WithCreationYear(match.Groups[2].Value);
                    break;
                default:
                    break;
            }
        }
    }

    private static void ExtractDataDuration(string responseContent, ref MovieBuilder movieBuilder)
    {
        var match = GetDurationPatternRegex().Match(responseContent);
        movieBuilder.WithDuration(match.Groups[1].Value);
    }

    private static void ExtractLongDescription(string responseContent, ref MovieBuilder movieBuilder)
    {
        var matches = GetLongDescriptionPatternRegex().Matches(responseContent);

        if (matches.Count == 1 && !string.IsNullOrEmpty(matches[0].Groups[1].Value))
        {
            var value = matches[0].Groups[1].Value;
            movieBuilder.WithDescription(HttpUtility.HtmlDecode(value));
        }
    }

    private static void ExtractGenres(ref MovieBuilder movieBuilder, HtmlDocument web)
    {
        var nodes = web.DocumentNode.SelectNodes("//div[@data-tag-type='rankingGenre']/span");
        foreach (var node in nodes)
        {
            var genre = HttpUtility.HtmlDecode(node.InnerText).Trim();
            movieBuilder.WithGenre(genre);
        }
    }

    private static void ExtractActors(string responseContent, ref MovieBuilder movieBuilder, HtmlDocument web)
    {
        var nodes = web.DocumentNode.SelectNodes("//section[contains(@class, 'FilmCastSection')]//h3[contains(@itemprop, 'name')]");

        foreach (var node in nodes)
        {
            var actor = HttpUtility.HtmlDecode(node.InnerText ?? string.Empty).Trim();
            movieBuilder.WithActor(actor);
        }

        if (nodes.Any())
            return;

        var matches = GetActorsPatternRegex().Matches(responseContent);

        foreach (Match match in matches)
        {
            var actor = match.Groups[1].Value;
            actor = actor.Replace("+", " ");
            actor = HttpUtility.UrlDecode(actor);
            movieBuilder.WithActor(actor);
        }

    }

    private static void ExtractWriters(string responseContent, ref MovieBuilder movieBuilder)
    {
        var matches = GetWritersPatternRegex().Matches(responseContent);
        foreach (Match match in matches)
        {
            var actor = match.Groups[1].Value;
            actor = actor.Replace("+", " ");
            actor = actor.TrimEnd('-');
            actor = HttpUtility.UrlDecode(actor);
            movieBuilder.WithWriter(actor);
        }
    }

    private static void ExtractDirectors(string responseContent, ref MovieBuilder movieBuilder)
    {
        var matches = GetDirectorsPatternRegex().Matches(responseContent);
        foreach (Match match in matches)
        {
            var actor = match.Groups[1].Value;
            actor = actor.Replace("+", " ");
            actor = actor.TrimEnd('-');
            actor = HttpUtility.UrlDecode(actor);
            movieBuilder.WithDirector(actor);
        }
    }

    public sealed class FilmwebInfoDTO
    {
        public string Title { get; set; }
        public string OriginalTitle { get; set; }
        public int Year { get; set; }
    }
}