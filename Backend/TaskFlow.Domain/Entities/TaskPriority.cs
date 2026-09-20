namespace TaskFlow.Domain.Entities;

public sealed class TaskPriority
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Level { get; set; }
}
