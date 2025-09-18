namespace Filmowanie.Account.DTOs.Outgoing;

public sealed record UserDTO(string Username, bool IsAdmin, bool HasRegisteredBasicAuth, string Gender);