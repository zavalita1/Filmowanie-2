namespace Filmowanie.Abstractions.Configuration;

public sealed class GoogleAuthOptions
{
    public required string TokenUri { get; set; }
    public required string AuthUri { get; set; }
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
    public required string RedirectUri { get; set; }
    public required string DiscoveryUri { get; set; }

    public bool Enabled { get; set; }
}