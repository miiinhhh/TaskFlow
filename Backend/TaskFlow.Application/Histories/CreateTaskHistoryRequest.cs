using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Application.Histories;

public sealed record CreateTaskHistoryRequest(
    [param: Required, StringLength(100)] string Action,
    [param: StringLength(4000)] string? OldValue,
    [param: StringLength(4000)] string? NewValue);
