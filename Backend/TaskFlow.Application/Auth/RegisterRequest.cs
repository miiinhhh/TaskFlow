using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Application.Auth;

public sealed record RegisterRequest(
    [property: Required, StringLength(100, MinimumLength = 3)] string Username,
    [property: Required, EmailAddress, StringLength(255)] string Email,
    [property: Required, StringLength(200, MinimumLength = 6)] string Password,
    [property: Required, StringLength(150)] string FullName,
    [property: Url, StringLength(500)] string? AvatarUrl);
