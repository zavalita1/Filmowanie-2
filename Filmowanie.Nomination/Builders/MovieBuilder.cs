using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;

namespace Filmowanie.Nomination.Builders;

internal sealed class MovieBuilder
{
    private string? movieName;
    private string? posterUrl;
    private string? bigPosterUrl;
    private string? description;
    private string? filmwebUrl;
    private int year;
    private TimeSpan duration;
    private readonly List<string> genres = new(4);
    private readonly List<string> actors = new(4);
    private readonly List<string> directors = new(4);
    private readonly List<string> writers = new(4);
    private string? originalTitle;
    private TenantId tenant;

    public MovieBuilder WithName(string name)
    {
        this.movieName = name;
        return this;
    }

    public MovieBuilder WithTenant(TenantId tenant)
    {
        this.tenant = tenant;
        return this;
    }

    public MovieBuilder WithOriginalTitle(string name)
    {
        originalTitle = name;
        return this;
    }

    public MovieBuilder WithPosterUrl(string posterUrl)
    {
        this.posterUrl = posterUrl;
        return this;
    }

    public MovieBuilder WithBigPosterUrl(string posterUrl)
    {
        bigPosterUrl = posterUrl;
        return this;
    }

    public MovieBuilder WithDescription(string description)
    {
        this.description = description;
        return this;
    }

    public MovieBuilder WithCreationYear(string year)
    {
        this.year = int.Parse(year);
        return this;
    }

    public MovieBuilder WithDuration(string minutes)
    {
        var minutesNo = int.Parse(minutes);
        duration = TimeSpan.FromMinutes(minutesNo);
        return this;
    }

    public MovieBuilder WithGenre(string genre)
    {
        genres.Add(genre);
        return this;
    }
    
    public MovieBuilder WithActor(string actor)
    {
        actors.Add(actor);
        return this;
    }
    
    public MovieBuilder WithDirector(string director)
    {
        directors.Add(director);
        return this;
    }

    public MovieBuilder WithWriter(string writer)
    {
        writers.Add(writer);
        return this;
    }

    public MovieBuilder WithFilmwebUrl(string filmwebUrl)
    {
        this.filmwebUrl = filmwebUrl;
        return this;
    }

    public IReadOnlyMovieEntity Build(IGuidProvider guidProvider, IDateTimeProvider dateTimeProvider)
    {
        if (string.IsNullOrEmpty(this.posterUrl) || string.IsNullOrEmpty(bigPosterUrl) || string.IsNullOrEmpty(this.description) || string.IsNullOrEmpty(this.description) || string.IsNullOrEmpty(this.movieName) || duration == default || year == default || string.IsNullOrEmpty(this.filmwebUrl) || this.tenant == default)
        {
            throw new ArgumentException("Cannot construct this!");
        }

        var originalTitle = this.originalTitle ?? this.movieName;
        var description = this.description.StartsWith($"{this.movieName} (")
            ? this.description[(this.movieName!.Length + 9)..]
            : this.description;

        var movieId = "movie-" + guidProvider.NewGuid();
        var now = dateTimeProvider.Now;

        return new Movie(movieId, now, this.movieName, originalTitle, description, this.posterUrl, this.bigPosterUrl, this.filmwebUrl, this.actors.ToArray(), this.writers.ToArray(), this.directors.ToArray(), this.genres.ToArray(), this.year, (int)this.duration.TotalMinutes, this.tenant.Id, "");
    }

    private readonly record struct Movie(string id, DateTime Created, string Name, string OriginalTitle, string Description, string PosterUrl, string BigPosterUrl, string FilmwebUrl, string[] Actors, string[] Writers, string[] Directors, string[] Genres, int CreationYear, int DurationInMinutes, int TenantId, string Type) : IReadOnlyMovieEntity
    {
        public bool? IsRejected => null;
    }
}