using Filmowanie.Account.DTOs.Incoming;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Filmowanie.Account.Interfaces;

internal interface IAccountRoutes
{
    public Task<IResult> LoginAsync([FromBody] LoginDto dto, CancellationToken cancel);
    public Task<IResult> LoginBasicAsync([FromBody] BasicAuthLoginDTO dto, CancellationToken cancel);
    public Task<IResult> LoginGoogleAsync([FromBody] GoogleOAuthClientDTO dto, CancellationToken cancel);
    public Task<IResult> SignUpAsync([FromBody] BasicAuthLoginDTO dto, CancellationToken cancel);
    public Task<IResult> LogoutAsync(CancellationToken cancel);
    public IResult Get(CancellationToken cancel);
}