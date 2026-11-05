namespace TaskFlow.Application.Projects;

public sealed record CreateProjectRequest(
    string Name,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate);