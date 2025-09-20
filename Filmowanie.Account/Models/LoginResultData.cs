using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Filmowanie.Account.Models;

internal readonly record struct LoginResultData(ClaimsIdentity Identity, AuthenticationProperties AuthenticationProperties);
