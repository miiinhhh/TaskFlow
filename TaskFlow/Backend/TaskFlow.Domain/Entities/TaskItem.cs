namespace TaskFlow.Domain.Entities;

public sealed class TaskItem
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int CreatedById { get; set; }
    public int StatusId { get; set; }
    public int PriorityId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal? ActualHours { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsDeleted { get; set; }

    public Project? Project { get; set; }
    public User? CreatedBy { get; set; }
    public TaskStatus? Status { get; set; }
    public TaskPriority? Priority { get; set; }
}
