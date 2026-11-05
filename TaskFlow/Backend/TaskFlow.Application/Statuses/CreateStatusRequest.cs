namespace TaskFlow.Application.Statuses;

public sealed record CreateStatusRequest(
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsCompleted);
