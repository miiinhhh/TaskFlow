namespace TaskFlow.Domain.Entities;

public sealed class ProjectMember
{
    public int ProjectId { get; set; }
    public int UserId { get; set; }
    public DateTime JoinedAt { get; set; }

    public Project? Project { get; set; }
    public User? User { get; set; }
}