namespace Filmowanie.Notification.DTOs.Incoming;

internal sealed class PushSubscriptionDTO
{
    public string Endpoint { get; set; }
    public string Client { get; set; }
    public string P256DH { get; set; }
    public string Auth { get; set; }
}