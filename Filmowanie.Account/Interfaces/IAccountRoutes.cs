using Filmowanie.Account.DTOs.Incoming;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Filmowanie.Account.Interfaces;

public interface IAccountRoutes
{
    Task<IResult> Login([FromBody] LoginDto dto, CancellationToken cancel);
    Task<IResult> LoginBasic([FromBody] BasicAuthLoginDTO dto, CancellationToken cancel);
    Task<IResult> SignUp([FromBody] BasicAuthLoginDTO dto, CancellationToken cancel);
    Task<IResult> Logout(CancellationToken cancel);
    Task<IResult> Get(CancellationToken cancel);
}