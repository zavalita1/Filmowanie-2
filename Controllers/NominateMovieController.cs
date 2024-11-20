using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Filmowanie.Account.Constants;
using Microsoft.AspNetCore.Authorization;
using Filmowanie.DTOs.Incoming;
using Microsoft.AspNetCore.SignalR;
using Filmowanie.DTOs.Outgoing;

namespace Filmowanie.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = Schemes.Cookie)]
[Route("[controller]")]
public sealed class NominateMovieController : ControllerBase
{
    public NominateMovieController()
    {
    }

    [HttpGet("getData")]
    public async Task<IActionResult> GetData(CancellationToken cancellationToken)
    {
        return Ok(); // TODO
    }

    [HttpPost("")]
    public async Task<IActionResult> Nominate([FromBody] NominationDTO dto, CancellationToken cancellation)
    {
        return Ok(); // TODO
    }
    
}