namespace Filmowanie.Abstractions;

public readonly record struct DomainUser(string Id, string Username, bool IsAdmin, bool HasBasicAuthSetup, int TenantId, DateTime Created);

public readonly record struct BasicAuth(string Email, string Password);