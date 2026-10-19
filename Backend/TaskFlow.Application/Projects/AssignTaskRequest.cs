using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Application.Projects;

public sealed record AssignTaskRequest([property: Range(1, int.MaxValue)] int UserId);
