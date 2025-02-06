namespace Filmowanie.Abstractions.Configuration;

public sealed class PushNotificationOptions
{
    public string Subject { get; set; }

    public string PublicKey { get; set; }

    public string PrivateKey { get; set; }
}