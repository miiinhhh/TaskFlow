namespace TaskFlow.Application.Tasks;

public sealed record UpdateTaskRequest(
    int ProjectId,
    int StatusId,
    int PriorityId,
    string Title,
    string? Description,
    DateTime? StartDate,
    DateTime? DueDate,
    decimal? EstimatedHours,
    decimal? ActualHours,
    DateTime? CompletedAt);
