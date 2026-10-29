namespace TaskFlow.Domain.Entities;

public sealed class TaskHistory
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public int UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime CreatedAt { get; set; }

    public TaskItem? Task { get; set; }
    public User? User { get; set; }
}
