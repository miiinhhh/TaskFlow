namespace TaskFlow.Application.Priorities;

public sealed record PriorityResponse(
    int Id,
    string Name,
    string? Description,
    int Level);
