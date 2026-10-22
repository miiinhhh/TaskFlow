using System.ComponentModel.DataAnnotations;
using TaskFlow.Application.Validation;

namespace TaskFlow.Application.Priorities;

public sealed record UpdatePriorityRequest(
    [param: Required, NotWhiteSpace, StringLength(50)] string Name,
    [param: StringLength(255)] string? Description,
    [param: Range(0, int.MaxValue)] int Level);
