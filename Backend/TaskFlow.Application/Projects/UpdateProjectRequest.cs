using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Application.Projects;

public sealed record UpdateProjectRequest(
    [param: Required, StringLength(200)] string Name,
    [param: StringLength(1000)] string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate,
    bool IsArchived);
