using System.Threading;
using System.Threading.Tasks;
using Filmowanie.Account.Constants;
using Filmowanie.DTOs.Incoming;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Filmowanie.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = SchemesNamesConsts.Cookie)]
[Route("[controller]")]
public class VotesController : ControllerBase
{
    public VotesController()
    {
    }

    [HttpGet("")]
    public async Task<IActionResult> Votes(CancellationToken cancel)
    {
        return Ok(); // TODO
    }

    [HttpGet("previousVotingSessions")]
    public async Task<IActionResult> Votes([FromQuery]int votingSessionsAgo, CancellationToken cancel)
    {
        return Ok(); // TODO
    }

    [HttpPost("vote")]
    public async Task<IActionResult> Vote([FromBody] VoteDTO vote, CancellationToken cancel)
    {
        return Ok(); // TODO
    }
}