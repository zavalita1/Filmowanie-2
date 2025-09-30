namespace Filmowanie.Abstractions.Configuration;

public sealed class FilmwebOptions
{
    public required string BaseUrl { get; set; }

    public required string FilmApiUrl { get; set; }
    public required string FallbackPosterUrl { get; set; }
    public required string FallbackBigPosterUrl { get; set; }
}
