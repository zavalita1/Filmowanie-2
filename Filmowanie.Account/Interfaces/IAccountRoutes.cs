using Filmowanie.Account.DTOs.Incoming;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Filmowanie.Account.Interfaces;

internal interface IAccountRoutes
{
    Task<IResult> LoginAsync([FromBody] LoginDto dto, CancellationToken cancel);
    Task<IResult> LoginBasicAsync([FromBody] BasicAuthLoginDTO dto, CancellationToken cancel);
    Task<IResult> SignUpAsync([FromBody] BasicAuthLoginDTO dto, CancellationToken cancel);
    Task<IResult> LogoutAsync(CancellationToken cancel);
    IResult Get(CancellationToken cancel);
}