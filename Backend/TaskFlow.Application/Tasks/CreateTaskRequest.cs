using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Application.Tasks;

public sealed record CreateTaskRequest(
    [property: Range(1, int.MaxValue)] int ProjectId,
    [property: Range(1, int.MaxValue)] int StatusId,
    [property: Range(1, int.MaxValue)] int PriorityId,
    [property: Required, StringLength(200)] string Title,
    [property: StringLength(4000)] string? Description,
    DateTime? StartDate,
    DateTime? DueDate,
    [property: Range(typeof(decimal), "0", "79228162514264337593543950335")] decimal? EstimatedHours,
    [property: Range(typeof(decimal), "0", "79228162514264337593543950335")] decimal? ActualHours);
