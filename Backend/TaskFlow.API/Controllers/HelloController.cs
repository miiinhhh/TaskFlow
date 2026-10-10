using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TaskFlow.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/hello")]
public sealed class HelloController : ControllerBase
{
    [HttpGet]
    [Produces("text/plain")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ContentResult Get()
    {
        return Content("Hello ASP.NET Core!", "text/plain");
    }
}