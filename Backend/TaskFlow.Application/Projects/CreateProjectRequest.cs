using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Application.Projects;

public sealed record CreateProjectRequest(
    [property: Required, StringLength(200)] string Name,
    [property: StringLength(1000)] string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate);
