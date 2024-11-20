namespace Filmowanie.DTOs.Outgoing;

public sealed record UserDTO(string Name, bool IsAdmin, bool HasRegisteredBasicAuth);