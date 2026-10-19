using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Application.Priorities;

public sealed record CreatePriorityRequest(
    [property: Required, StringLength(50)] string Name,
    [property: StringLength(255)] string? Description,
    [property: Range(0, int.MaxValue)] int Level);
