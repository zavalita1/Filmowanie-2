using System.Text.Json.Serialization;

namespace Filmowanie.Account.DTOs.Incoming;

public sealed class GoogleUserInfoResponseDTO
{
    [JsonPropertyName("email_verified")]
    public bool EmailVerified { get; set; }
    
    [JsonPropertyName("sub")]
    public string Sub { get; set; } = null!;

    [JsonPropertyName("given_name")]
    public string GivenName { get; set; } = null!;

    [JsonPropertyName("family_name")]
    public string FamilyName { get; set; } = null!;

    [JsonPropertyName("picture")]
    public string PictureUrl { get; set; } = null!;

    [JsonPropertyName("email")]
    public string Mail { get; set; } = null!;
}