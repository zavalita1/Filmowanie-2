namespace Filmowanie.Abstractions.Configuration;

public sealed class PushNotificationOptions
{
    public required string Subject { get; set; }

    public required string PublicKey { get; set; }

    public required string PrivateKey { get; set; }

    public required bool Enabled { get; set; }
}
