using System.ComponentModel.DataAnnotations;
using TaskFlow.Application.Validation;

namespace TaskFlow.Application.Auth;

public sealed record RegisterRequest(
    [param: Required, NotWhiteSpace, StringLength(100, MinimumLength = 3)] string Username,
    [param: Required, NotWhiteSpace, EmailAddress, StringLength(255)] string Email,
    [param: Required, NotWhiteSpace, StringLength(200, MinimumLength = 6)] string Password,
    [param: Required, NotWhiteSpace, StringLength(150)] string FullName,
    [param: Url, StringLength(500)] string? AvatarUrl);
