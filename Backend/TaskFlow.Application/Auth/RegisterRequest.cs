using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Application.Auth;

public sealed record RegisterRequest(
    [param: Required, StringLength(100, MinimumLength = 3)] string Username,
    [param: Required, EmailAddress, StringLength(255)] string Email,
    [param: Required, StringLength(200, MinimumLength = 6)] string Password,
    [param: Required, StringLength(150)] string FullName,
    [param: Url, StringLength(500)] string? AvatarUrl);
