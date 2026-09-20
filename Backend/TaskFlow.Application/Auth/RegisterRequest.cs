namespace TaskFlow.Application.Auth;

public sealed record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string FullName,
    int? RoleId,
    string? AvatarUrl);
