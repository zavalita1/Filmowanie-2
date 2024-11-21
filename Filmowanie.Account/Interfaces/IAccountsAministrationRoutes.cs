using Filmowanie.Account.DTOs.Incoming;
using Microsoft.AspNetCore.Http;

namespace Filmowanie.Account.Interfaces;

internal interface IAccountsAministrationRoutes
{
    Task<IResult> GetUsers(CancellationToken cancel);
    Task<IResult> GetUser(string userId, CancellationToken cancel);
    Task<IResult> AddUser(UserDTO dto, CancellationToken cancel);
}