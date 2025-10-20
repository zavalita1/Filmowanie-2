using Filmowanie.Abstractions.Configuration;
using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Filmowanie.Nomination.Builders;
using Filmowanie.Nomination.Consts;
using Filmowanie.Nomination.DTOs.Incoming;
using Filmowanie.Nomination.Interfaces;
using HtmlAgilityPack;
using MassTransit.Internals.GraphValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Web;

namespace Filmowanie.Nomination.Services;

// TODO refactor to ditch regexes, write UTs
internal sealed partial class FilmwebHandler : IFilmwebHandler
{
    private readonly IHttpClientFactory clientFactory;
    private readonly IGuidProvider guidProvider;
    private readonly IDateTimeProvider dateTimeProvider;
    private readonly IFilmwebPathResolver filmwebPathResolver;
    private readonly IOptions<FilmwebOptions> options;
    private readonly ILogger<FilmwebHandler> log;

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


    public FilmwebHandler(IHttpClientFactory clientFactory, IGuidProvider guidProvider, IDateTimeProvider dateTimeProvider, IFilmwebPathResolver filmwebPathResolver, ILogger<FilmwebHandler> log, IOptions<FilmwebOptions> options)
    {
        this.clientFactory = clientFactory;
        this.guidProvider = guidProvider;
        this.dateTimeProvider = dateTimeProvider;
        this.filmwebPathResolver = filmwebPathResolver;
        this.log = log;
        this.options = options;
    }

    public Task<Maybe<IReadOnlyMovieEntity>> GetMovieAsync(Maybe<(NominationDTO NominationDto, DomainUser CurrentUser)> input, CancellationToken cancel) =>
        input.AcceptAsync(GetMovieAsync, this.log, cancel);


    public async Task<Maybe<IReadOnlyMovieEntity>> GetMovieAsync((NominationDTO NominationDto, DomainUser CurrentUser) input, CancellationToken cancel)
    {
        var metadata = this.filmwebPathResolver.GetMetadata(input.NominationDto.MovieFilmwebUrl);
        var client = this.clientFactory.CreateClient(HttpClientNames.Filmweb);
        var metadataRoute = $"{this.options.Value.FilmApiUrl}{metadata.MovieId}/info";
        using var apiRequest = new HttpRequestMessage(HttpMethod.Get, metadataRoute);
        apiRequest.Headers.Add("x-locale", "pl");
        var baseUrl = new Uri(this.options.Value.BaseUrl, UriKind.Absolute);
        var moviePath = new Uri(baseUrl, metadata.MovieRelativePath);
        
        var tasks = new [] { 
            client.GetAsync(moviePath, cancel),
            client.SendAsync(apiRequest, cancel)};

        var responses = await Task.WhenAll(tasks);

        if (!responses[0].IsSuccessStatusCode || !responses[1].IsSuccessStatusCode)
            return new Error<IReadOnlyMovieEntity>("Cannot access filmweb. Check if the link provided is correct. If it is, try again. If this issue persists, contact admin.", ErrorType.Network);

        var responsesContentTask = responses[0].Content.ReadAsStringAsync(cancel);
        var responsesContent2Task = responses[1].Content.ReadFromJsonAsync<FilmwebInfoDto>(cancellationToken: cancel);

        var responseContent = await responsesContentTask;
        var responseContent2 = await responsesContent2Task;

        var movieBuilder = new MovieBuilder(this.options, this.log).WithFilmwebUrl(metadata.MovieAbsolutePath);
        var web = new HtmlDocument();
        web.LoadHtml(responseContent);

        ExtractDataFromMetaElements(responseContent, ref movieBuilder);
        ExtractDataFromTitleDetailsElements(responseContent, ref movieBuilder);
        ExtractDataDuration(responseContent, ref movieBuilder);
        ExtractLongDescription(ref movieBuilder, web);
        ExtractGenres(ref movieBuilder, web);
        ExtractActors(responseContent, ref movieBuilder, web);
        ExtractDirectors(responseContent, ref movieBuilder);
        ExtractWriters(responseContent, ref movieBuilder);

        if (!string.IsNullOrEmpty(responseContent2!.Title))
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

        return movieBuilder.Build(this.guidProvider, this.dateTimeProvider).AsMaybe();
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
        var sizePartIndex = posterUrl[..extensionIndex].LastIndexOf(".");
        var size = isBigPoster ? "8" : "10";
        posterUrl = $"{posterUrl[..sizePartIndex]}.{size}{posterUrl[extensionIndex..]}";
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

    private void ExtractLongDescription(ref MovieBuilder movieBuilder, HtmlDocument web)
    {
        var nodes = web.DocumentNode.SelectNodes("//div/span[@itemprop='description']");
         
        if (nodes?.Count() != 1)
        {
            this.log.LogWarning("Unexpected elements found when trying to fetch description!");
            return;
        }

        var description = HttpUtility.HtmlDecode(nodes.Single().InnerText).Trim();
        movieBuilder.WithDescription(description);
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

    public sealed class FilmwebInfoDto
    {
        public string Title { get; set; } = null!;
        public string OriginalTitle { get; set; } = null!;
        public int Year { get; set; }
    }
}