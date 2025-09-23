using Filmowanie.Account.Interfaces;

namespace Filmowanie.Account.Models;

public readonly record struct BasicAuthUserData(string Email, string Password) : IMailBasedUserData;