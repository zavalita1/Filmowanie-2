using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using Filmowanie.Account.Constants;
using Filmowanie.DTOs.Outgoing;
using Microsoft.Extensions.DependencyInjection;

namespace Filmowanie.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = Schemes.Cookie)]
public sealed class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _log;
    private readonly IServiceProvider _serviceProvider;

    public UsersController(ILogger<UsersController> log, IServiceProvider serviceProvider)
    {
        _log = log;
        _serviceProvider = serviceProvider;
    }

    [HttpGet("")]
    [Authorize(Schemes.Admin)]
    public async Task<IActionResult> GetUsers(CancellationToken cancel)
    {
        return Ok(); // TODO
    }

    [HttpGet("{name}")]
    [Authorize(Schemes.Admin)]
    public async Task<IActionResult> GetUser([FromRoute] string name, CancellationToken cancel)
    {
        return Ok(); // TODO
    }

    // TODO add mappers layer
    [HttpPost("{userName}")]
    [Authorize(Schemes.Admin)]
    public async Task<IActionResult> AddUser([FromRoute]string userName, CancellationToken cancel)
    {
        return Ok(); // TODO
    }


}