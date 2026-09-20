using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Tasks;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Tasks;

public sealed class TaskService(TaskFlowDbContext dbContext) : ITaskService
{
    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = new TaskItem
        {
            ProjectId = request.ProjectId,
            CreatedById = request.CreatedById,
            StatusId = request.StatusId,
            PriorityId = request.PriorityId,
            Title = request.Title.Trim(),
            Description = request.Description,
            StartDate = request.StartDate,
            DueDate = request.DueDate,
            EstimatedHours = request.EstimatedHours,
            ActualHours = request.ActualHours,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Tasks.Add(task);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await ProjectTask()
            .SingleAsync(t => t.Id == task.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<TaskResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await ProjectTask()
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<TaskResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return ProjectTask()
            .SingleOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<TaskResponse?> UpdateAsync(int id, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = await dbContext.Tasks
            .SingleOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);

        if (task is null)
        {
            return null;
        }

        task.ProjectId = request.ProjectId;
        task.StatusId = request.StatusId;
        task.PriorityId = request.PriorityId;
        task.Title = request.Title.Trim();
        task.Description = request.Description;
        task.StartDate = request.StartDate;
        task.DueDate = request.DueDate;
        task.EstimatedHours = request.EstimatedHours;
        task.ActualHours = request.ActualHours;
        task.CompletedAt = request.CompletedAt;
        task.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return await ProjectTask()
            .SingleAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await dbContext.Tasks
            .SingleOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);

        if (task is null)
        {
            return false;
        }

        task.IsDeleted = true;
        task.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private IQueryable<TaskResponse> ProjectTask()
    {
        return dbContext.Tasks
            .AsNoTracking()
            .Where(t => !t.IsDeleted)
            .Select(t => new TaskResponse(
                t.Id,
                t.ProjectId,
                t.Project == null ? null : t.Project.Name,
                t.CreatedById,
                t.CreatedBy == null ? null : t.CreatedBy.FullName,
                t.StatusId,
                t.Status == null ? null : t.Status.Name,
                t.PriorityId,
                t.Priority == null ? null : t.Priority.Name,
                t.Title,
                t.Description,
                t.StartDate,
                t.DueDate,
                t.EstimatedHours,
                t.ActualHours,
                t.CreatedAt,
                t.UpdatedAt,
                t.CompletedAt));
    }
}
