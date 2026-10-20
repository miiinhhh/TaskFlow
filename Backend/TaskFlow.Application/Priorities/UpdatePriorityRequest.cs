using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Application.Priorities;

public sealed record UpdatePriorityRequest(
    [param: Required, StringLength(50)] string Name,
    [param: StringLength(255)] string? Description,
    [param: Range(0, int.MaxValue)] int Level);
