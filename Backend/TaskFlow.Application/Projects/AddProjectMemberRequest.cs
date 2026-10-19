using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Application.Projects;

public sealed record AddProjectMemberRequest([property: Range(1, int.MaxValue)] int UserId);
