using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Application.Histories;

public sealed record CreateTaskHistoryRequest(
    [property: Required, StringLength(100)] string Action,
    [property: StringLength(4000)] string? OldValue,
    [property: StringLength(4000)] string? NewValue);
