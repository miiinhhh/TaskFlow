namespace TaskFlow.Application.Tasks;

public sealed record TaskResponse(
    int Id,
    int ProjectId,
    string? ProjectName,
    int CreatedById,
    string? CreatedByName,
    int StatusId,
    string? StatusName,
    int PriorityId,
    string? PriorityName,
    string Title,
    string? Description,
    DateTime? StartDate,
    DateTime? DueDate,
    decimal? EstimatedHours,
    decimal? ActualHours,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? CompletedAt);
