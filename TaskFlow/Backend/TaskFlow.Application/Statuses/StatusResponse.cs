namespace TaskFlow.Application.Statuses;

public sealed record StatusResponse(
    int Id,
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsCompleted);
