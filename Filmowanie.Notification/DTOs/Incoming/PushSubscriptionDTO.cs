namespace Filmowanie.Notification.DTOs.Incoming;

internal sealed class PushSubscriptionDTO
{
    public string Endpoint { get; set; }
    public string p256dh { get; set; }
    public string Auth { get; set; }
}