using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Application.Auth;

public sealed record LoginRequest(
    [param: Required, StringLength(255)] string UsernameOrEmail,
    [param: Required, StringLength(200)] string Password);
