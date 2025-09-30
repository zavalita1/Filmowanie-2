using System.Text.Json.Serialization;

namespace Filmowanie.Voting.DTOs.Incoming;


public sealed class ImdbMovieSearchResponseDTO
{
    [JsonPropertyName("d")]
    public List<MovieItem> Results { get; set; } = new();

    [JsonPropertyName("q")]
    public string Query { get; set; } = string.Empty;

    [JsonPropertyName("v")]
    public int Version { get; set; }
}

public sealed class MovieItem
{
    [JsonPropertyName("i")]
    public MovieImage Image { get; set; } = new();

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("l")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("q")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("qid")]
    public string TypeId { get; set; } = string.Empty;

    [JsonPropertyName("rank")]
    public int Rank { get; set; }

    [JsonPropertyName("s")]
    public string Cast { get; set; } = string.Empty;

    [JsonPropertyName("vt")]
    public int VideoType { get; set; }

    [JsonPropertyName("y")]
    public int Year { get; set; }
}

public class MovieImage
{
    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("width")]
    public int Width { get; set; }
}