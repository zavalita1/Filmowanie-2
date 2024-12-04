using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Filmowanie.Abstractions.Constants;
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

    [HttpDelete("{movieName}")]
    [Authorize(Schemes.Admin)]
    public async Task<IActionResult> Delete([FromRoute] string movieName, CancellationToken cancellation)
    {
        return Ok(); // TODO
    }

}