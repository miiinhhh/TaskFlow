namespace TaskFlow.Domain.Entities;

public sealed class TaskAssignment
{
    public int TaskId { get; set; }
    public int UserId { get; set; }
    public DateTime AssignedAt { get; set; }
    public int? AssignedById { get; set; }

    public TaskItem? Task { get; set; }
    public User? User { get; set; }
    public User? AssignedBy { get; set; }
}