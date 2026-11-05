namespace TaskFlow.Application.Statuses;

public sealed record UpdateStatusRequest(
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsCompleted);
