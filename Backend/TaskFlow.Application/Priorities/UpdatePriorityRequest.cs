namespace TaskFlow.Application.Priorities;

public sealed record UpdatePriorityRequest(
    string Name,
    string? Description,
    int Level);
