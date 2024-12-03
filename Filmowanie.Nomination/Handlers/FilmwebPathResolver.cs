using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Filmowanie.Nomination.Consts;
using Filmowanie.Nomination.Interfaces;

namespace Filmowanie.Nomination.Handlers;

internal sealed class FilmwebPathResolver : IFilmwebPathResolver
{
    public FilmwebUriMetadata GetMetadata(string filmbwebMovieAbsolutePath)
    {
        var sanitizedAbsolutePath = GetSanitizedAbsolutePath(filmbwebMovieAbsolutePath);
        var relativePath = sanitizedAbsolutePath.Absolute[Urls.FilmwebUrl.Length..];


        return new FilmwebUriMetadata(relativePath, sanitizedAbsolutePath.Absolute, sanitizedAbsolutePath.Id);
    }

    private static (string Absolute, int Id) GetSanitizedAbsolutePath(string filmwebAbsolutePath)
    {
        var escapedUrl = Regex.Escape(Urls.FilmwebUrl);
        var match = Regex.Match(filmwebAbsolutePath, $"({escapedUrl}film/([^\\?]*))");
        var id = match.Groups[2].Value.Split('-').Last();

        if (!int.TryParse(id, out var parsedId))
            throw new ValidationException("Cannot get film id!");

        return (match.Groups[1].Value, parsedId);
    }
}

public sealed record FilmwebUriMetadata(string MovieRelativePath, string MovieAbsolutePath, int MovieId);