namespace TaskFlow.Application.Auth;

public sealed record AuthUserResponse(
    int Id,
    string Username,
    string Email,
    string FullName,
    string? AvatarUrl,
    int RoleId,
    string? RoleName);
