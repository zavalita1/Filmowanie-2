namespace Filmowanie.Abstractions.DomainModels;

public readonly record struct BasicAuthUserData(string Email, string Password) : IMailBasedUserData;