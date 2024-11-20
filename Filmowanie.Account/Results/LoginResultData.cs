using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Filmowanie.Account.Results;

public sealed record LoginResultData(ClaimsIdentity Identity, AuthenticationProperties AuthenticationProperties);
