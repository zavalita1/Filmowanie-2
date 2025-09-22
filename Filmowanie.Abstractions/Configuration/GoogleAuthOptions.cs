namespace Filmowanie.Abstractions.Configuration;

public sealed class GoogleAuthOptions
{
    public string TokenUri { get; set; }
    public string AuthUri { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string RedirectUri { get; set; }
    public string DiscoveryUri { get; set; }

    public bool Enabled { get; set; }
}