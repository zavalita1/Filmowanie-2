using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Filmowanie.Account.Constants;
using Filmowanie.DTOs.Outgoing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Filmowanie.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = SchemesNamesConsts.Cookie)]
[Route("[controller]")]
public class HistoryController : ControllerBase
{
    public HistoryController()
    {
    }

    [HttpGet("list")]
    public async Task<IActionResult> History(CancellationToken cancellation)
    {
        return Ok(); // TODO
    }

    [HttpGet("laststandings/{standings}")]
    public async Task<IActionResult> LastTenStandings([FromRoute] int standings, CancellationToken cancellation)
    {
        return Ok(); // TODO
    }

    [HttpGet("previousVotingSessions")]
    public async Task<IActionResult> PreviousVotingSessions(CancellationToken cancellation)
    {
        return Ok(); // TODO
    }
}