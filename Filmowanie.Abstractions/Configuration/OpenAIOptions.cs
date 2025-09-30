namespace Filmowanie.Abstractions.Configuration;

public sealed class OpenAIOptions
{
    public required string ApiKey { get; set; }

    public required bool Enabled { get; set; }
}