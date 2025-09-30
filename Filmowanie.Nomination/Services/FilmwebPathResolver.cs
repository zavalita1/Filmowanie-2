using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Filmowanie.Abstractions.Configuration;
using Filmowanie.Nomination.Interfaces;
using Microsoft.Extensions.Options;

namespace Filmowanie.Nomination.Services;

internal sealed class FilmwebPathResolver : IFilmwebPathResolver
{
    private readonly IOptions<FilmwebOptions> options;

    public FilmwebPathResolver(IOptions<FilmwebOptions> options)
    {
        this.options = options;
    }

    public FilmwebUriMetadata GetMetadata(string filmbwebMovieAbsolutePath)
    {
        var sanitizedAbsolutePath = GetSanitizedAbsolutePath(filmbwebMovieAbsolutePath);
        var relativePath = sanitizedAbsolutePath.Absolute[this.options.Value.BaseUrl.Length..];

        return new FilmwebUriMetadata(relativePath, sanitizedAbsolutePath.Absolute, sanitizedAbsolutePath.Id);
    }

    private (string Absolute, int Id) GetSanitizedAbsolutePath(string filmwebAbsolutePath)
    {
        var escapedUrl = Regex.Escape(this.options.Value.BaseUrl);
        var match = Regex.Match(filmwebAbsolutePath, $"({escapedUrl}film/([^\\?]*))");
        var id = match.Groups[2].Value.Split('-').Last();

        if (!int.TryParse(id, out var parsedId))
            throw new ValidationException("Cannot get film id!");

        return (match.Groups[1].Value, parsedId);
    }
}

public sealed record FilmwebUriMetadata(string MovieRelativePath, string MovieAbsolutePath, int MovieId);