using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Application.Tasks;

public sealed record CreateTaskRequest(
    [param: Range(1, int.MaxValue)] int ProjectId,
    [param: Range(1, int.MaxValue)] int StatusId,
    [param: Range(1, int.MaxValue)] int PriorityId,
    [param: Required, StringLength(200)] string Title,
    [param: StringLength(4000)] string? Description,
    DateTime? StartDate,
    DateTime? DueDate,
    [param: Range(typeof(decimal), "0", "79228162514264337593543950335")] decimal? EstimatedHours,
    [param: Range(typeof(decimal), "0", "79228162514264337593543950335")] decimal? ActualHours);
