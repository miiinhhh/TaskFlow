namespace TaskFlow.Application.Tasks;

public sealed record CreateTaskRequest(
    int ProjectId,
    int CreatedById,
    int StatusId,
    int PriorityId,
    string Title,
    string? Description,
    DateTime? StartDate,
    DateTime? DueDate,
    decimal? EstimatedHours,
    decimal? ActualHours);
