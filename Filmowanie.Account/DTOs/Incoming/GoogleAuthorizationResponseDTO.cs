using System.Text.Json.Serialization;

namespace Filmowanie.Account.DTOs.Incoming;

public sealed class GoogleAuthorizationResponseDTO
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = null!;

    [JsonPropertyName("expires_in")]
    public int ExpiresInSeconds { get; set; }

    [JsonPropertyName("refresh_token_expires_in")]
    public int RefreshExpiresInSeconds { get; set; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = null!;

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = null!;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = null!;
}
