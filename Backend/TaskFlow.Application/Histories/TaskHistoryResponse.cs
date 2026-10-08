namespace TaskFlow.Application.Histories;

public sealed record TaskHistoryResponse(
    int Id,
    int TaskId,
    string? TaskTitle,
    int UserId,
    string? UserFullName,
    string Action,
    string? OldValue,
    string? NewValue,
    DateTime CreatedAt);
