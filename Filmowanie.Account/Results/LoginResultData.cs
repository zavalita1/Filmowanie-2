using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Filmowanie.Account.Results;

internal sealed record LoginResultData(ClaimsIdentity Identity, AuthenticationProperties AuthenticationProperties);
