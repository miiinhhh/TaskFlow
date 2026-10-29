using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TaskFlow.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/hello")]
public class HelloController : ControllerBase
{
    [HttpGet]
    [Produces("text/plain")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Content("Hello ASP.NET Core!", "text/plain");
    }

    [HttpGet("health")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "ok",
            service = "TaskFlow API",
            timestamp = DateTimeOffset.UtcNow
        });
    }
}
