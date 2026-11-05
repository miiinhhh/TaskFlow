namespace TaskFlow.Application.Projects;

public sealed record TaskAssignmentResponse(
    int TaskId,
    int ProjectId,
    int UserId,
    string Username,
    string FullName,
    int AssignedById,
    DateTime AssignedAt);