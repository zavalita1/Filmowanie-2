namespace Filmowanie.Abstractions.DomainModels;

public readonly record struct DomainUser(string Id, string Name, bool IsAdmin, bool HasBasicAuthSetup, TenantId Tenant, DateTime Created);