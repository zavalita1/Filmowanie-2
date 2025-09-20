using Filmowanie.Abstractions.DomainModels;

namespace Filmowanie.Account.Models;

public readonly record struct GoogleUserData(string Email, string DisplayName, string AccessToken, string RefreshToken): IMailBasedUserData;