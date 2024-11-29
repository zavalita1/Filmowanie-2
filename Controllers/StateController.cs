using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Filmowanie.Account.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;

namespace Filmowanie.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = Schemes.Cookie)]
public sealed class StateController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StateController> _log;

    public StateController(IServiceProvider serviceProvider, ILogger<StateController> log)
    {
        _serviceProvider = serviceProvider;
        _log = log;
    }

    [HttpPost("endVote")]
    [Authorize(Schemes.Admin)]
    public async Task<IActionResult> End(CancellationToken cancel)
    {
        return Ok();
    }

    [HttpPost("startVote")]
    [Authorize(Schemes.Admin)]
    public async Task<IActionResult> Start(CancellationToken cancel)
    {
        return Ok();
    }

    [HttpPost("newVote")]
    [Authorize(Schemes.Admin)]
    public async Task<IActionResult> NewVote(CancellationToken cancel)
    {
        return Ok();
    }


    [HttpGet("")]
    public async Task<IActionResult> State(CancellationToken cancel)
    {
        return Ok(); // TODO
    }
}