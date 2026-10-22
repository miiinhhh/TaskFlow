using System.ComponentModel.DataAnnotations;
using TaskFlow.Application.Validation;

namespace TaskFlow.Application.Projects;

[DateRange(nameof(StartDate), nameof(EndDate))]
public sealed record CreateProjectRequest(
    [param: Required, NotWhiteSpace, StringLength(200)] string Name,
    [param: StringLength(1000)] string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate);
