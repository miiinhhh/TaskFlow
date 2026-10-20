using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Application.Projects;

public sealed record AssignTaskRequest([param: Range(1, int.MaxValue)] int UserId);
