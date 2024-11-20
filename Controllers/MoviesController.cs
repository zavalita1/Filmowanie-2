using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Filmowanie.Account.Constants;
using Filmowanie.DTOs.Outgoing;
using Microsoft.AspNetCore.Authorization;

namespace Filmowanie.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = Schemes.Cookie)]
[Route("[controller]")]
public class MoviesController : ControllerBase
{
    public MoviesController()
    {
    }

    [HttpGet("list")]
    public async Task<IActionResult> List(CancellationToken cancellation)
    {
        return Ok(); // TODO
    }

    [HttpDelete("{movieName}")]
    [Authorize(Schemes.Admin)]
    public async Task<IActionResult> Delete([FromRoute] string movieName, CancellationToken cancellation)
    {
        return Ok(); // TODO
    }

    [HttpGet(template: "posters")]
    public async Task<IActionResult> GetPosterUrls([FromQuery] string movieUrl, CancellationToken cancellation)
    {
        return Ok(); // TODO
    }
}