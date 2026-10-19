using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Application.Auth;

public sealed record LoginRequest(
    [property: Required, StringLength(255)] string UsernameOrEmail,
    [property: Required, StringLength(200)] string Password);
