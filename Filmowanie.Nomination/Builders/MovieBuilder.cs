using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Filmowanie.Nomination.Builders;

public sealed class MovieBuilder
{
    private string? _movieName;
    private string? _posterUrl;
    private string? _description;
    private string? _filmwebUrl;
    private int _year;
    private TimeSpan _duration;
    private readonly List<string> _genres = new(4);
    private readonly List<string> _actors = new(4);
    private readonly List<string> _directors = new(4);
    private readonly List<string> _writers = new(4);
    private string? _originalTitle;
    private TenantId _tenant;


    public MovieBuilder WithName(string name)
    {
        _movieName = name;
        return this;
    }

    public MovieBuilder WithTenant(TenantId tenant)
    {
        _tenant = tenant;
        return this;
    }

    public MovieBuilder WithOriginalTitle(string name)
    {
        _originalTitle = name;
        return this;
    }

    public MovieBuilder WithPosterUrl(string posterUrl)
    {
        _posterUrl = posterUrl;
        return this;
    }

    public MovieBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public MovieBuilder WithCreationYear(string year)
    {
        _year = int.Parse(year);
        return this;
    }

    public MovieBuilder WithDuration(string minutes)
    {
        var minutesNo = int.Parse(minutes);
        _duration = TimeSpan.FromMinutes(minutesNo);
        return this;
    }

    public MovieBuilder WithGenre(string genre)
    {
        _genres.Add(genre);
        return this;
    }
    
    public MovieBuilder WithActor(string actor)
    {
        _actors.Add(actor);
        return this;
    }
    
    public MovieBuilder WithDirector(string director)
    {
        _directors.Add(director);
        return this;
    }

    public MovieBuilder WithWriter(string writer)
    {
        _writers.Add(writer);
        return this;
    }

    public MovieBuilder WithFilmwebUrl(string filmwebUrl)
    {
        _filmwebUrl = filmwebUrl;
        return this;
    }

    public MovieBuilder FromOther(IReadOnlyMovieEntity other)
    {
        var result = WithName(other.Name)
            .WithDescription(other.Description)
            .WithTenant(new TenantId(other.TenantId))
            .WithOriginalTitle(other.OriginalTitle)
            .WithPosterUrl(other.PosterUrl)
            .WithCreationYear(other.CreationYear.ToString())
            .WithDuration(other.DurationInMinutes.ToString())
            .WithFilmwebUrl(other.FilmwebUrl);

        foreach (var actor in other.Actors)
        {
            result = result.WithActor(actor);
        }

        foreach (var director in other.Directors)
        {
            result = result.WithDirector(director);
        }

        foreach (var writer in other.Writers)
        {
            result = result.WithWriter(writer);
        }

        foreach (var genre in other.Genres)
        {
            result = result.WithGenre(genre);
        }

        return result;
    }

    public IReadOnlyMovieEntity Build(IGuidProvider guidProvider, IDateTimeProvider dateTimeProvider)
    {
        if (string.IsNullOrEmpty(_posterUrl) || string.IsNullOrEmpty(_description) || string.IsNullOrEmpty(_description) || string.IsNullOrEmpty(_movieName) || _duration == default || _year == default || string.IsNullOrEmpty(_filmwebUrl) || _tenant == default)
        {
            throw new ArgumentException("Cannot construct this!");
        }

        var originalTitle = _originalTitle ?? _movieName;
        var description = _description.StartsWith($"{_movieName} (")
            ? _description[(_movieName!.Length + 9)..]
            : _description;

        var movieId = "movie-" + guidProvider.NewGuid();
        var now = dateTimeProvider.Now;

        return new Movie(movieId, now, _movieName, originalTitle, description, _posterUrl, _filmwebUrl, _actors.ToArray(), _writers.ToArray(), _directors.ToArray(), _genres.ToArray(), _year, (int)_duration.TotalMinutes, _tenant.Id, "");
    }

    private readonly record struct Movie(string id, DateTime Created, string Name, string OriginalTitle, string Description, string PosterUrl, string FilmwebUrl, string[] Actors, string[] Writers, string[] Directors, string[] Genres, int CreationYear, int DurationInMinutes, int TenantId, string Type) : IReadOnlyMovieEntity;
}