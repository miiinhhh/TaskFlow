namespace TaskFlow.Application.Priorities;

public sealed record CreatePriorityRequest(
    string Name,
    string? Description,
    int Level);
