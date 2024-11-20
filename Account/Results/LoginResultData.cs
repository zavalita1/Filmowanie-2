using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Filmowanie.Account.Results;

public sealed record LoginResultData(ClaimsIdentity Identity, AuthenticationProperties AuthenticationProperties);
