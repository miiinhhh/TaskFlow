namespace TaskFlow.Application.Projects;

public sealed record ProjectMemberResponse(
    int ProjectId,
    int UserId,
    string Username,
    string FullName,
    string? RoleName,
    DateTime JoinedAt);