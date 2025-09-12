using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Filmowanie.Account.Models;

internal sealed record LoginResultData(ClaimsIdentity Identity, AuthenticationProperties AuthenticationProperties);
