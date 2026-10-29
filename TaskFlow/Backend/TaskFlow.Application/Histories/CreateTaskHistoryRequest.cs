namespace TaskFlow.Application.Histories;

public sealed record CreateTaskHistoryRequest(
    int TaskId,
    int UserId,
    string Action,
    string? OldValue,
    string? NewValue);
