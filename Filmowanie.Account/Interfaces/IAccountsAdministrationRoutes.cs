using Filmowanie.Account.DTOs.Incoming;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Account.Interfaces;

internal interface IAccountsAdministrationRoutes
{
    Task<IResult> GetUsersAsync(CancellationToken cancel);
    Task<IResult> GetUserAsync(string userId, CancellationToken cancel);
    Task<IResult> AddUserAsync(CreateUserDTO dto, CancellationToken cancel);
}