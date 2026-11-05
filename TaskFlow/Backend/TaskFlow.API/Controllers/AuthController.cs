using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Auth;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username)
            || string.IsNullOrWhiteSpace(request.Email)
            || string.IsNullOrWhiteSpace(request.Password)
            || string.IsNullOrWhiteSpace(request.FullName))
        {
            return BadRequest("Username, email, password, and full name are required.");
        }

        if (request.Password.Length < 6)
        {
            return BadRequest("Password must be at least 6 characters.");
        }

        var result = await authService.RegisterAsync(request, cancellationToken);
        if (!result.Succeeded || result.Login is null)
        {
            return BadRequest(result.Error ?? "Register failed.");
        }

        return CreatedAtAction(nameof(Register), result.Login);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UsernameOrEmail) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Username/email and password are required.");
        }

        var login = await authService.LoginAsync(request, cancellationToken);
        return login is null ? Unauthorized() : Ok(login);
    }
}
