using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Application.Statuses;

public sealed record CreateStatusRequest(
    [property: Required, StringLength(50)] string Name,
    [property: StringLength(255)] string? Description,
    [property: Range(0, int.MaxValue)] int DisplayOrder,
    bool IsCompleted);
