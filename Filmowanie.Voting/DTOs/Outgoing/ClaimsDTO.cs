namespace Filmowanie.DTOs.Outgoing;

public sealed record ClaimsDTO(string Username, bool IsAdmin, bool HasNominations, bool HasBasicAuthSetup);
