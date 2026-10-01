namespace TaskFlow.Application.Projects;

public sealed record ProjectResponse(
    int Id,
    int OwnerId,
    string? OwnerName,
    string Name,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate,
    bool IsArchived,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    int MemberCount);