namespace Filmowanie.Account.DTOs.Outgoing;

public sealed record DetailedUserDTO(string Username, bool IsAdmin, bool HasRegisteredBasicAuth, int TenantId, string Code);