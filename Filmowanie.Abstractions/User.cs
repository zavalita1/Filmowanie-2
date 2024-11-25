namespace Filmowanie.Abstractions;

public readonly record struct DomainUser(string Id, string Username, bool IsAdmin, bool HasBasicAuthSetup, TenantId Tenant, DateTime Created);

public readonly record struct TenantId(int Id);
public readonly record struct VotingSession(string Id, DateTime? CompletedDateTime);

public readonly record struct BasicAuth(string Email, string Password);