using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Tasks;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Tasks;

public sealed class TaskService(TaskFlowDbContext dbContext) : ITaskService
{
    public async Task<TaskResponse?> CreateAsync(CreateTaskRequest request, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        if (!isAdmin && !await CanManageProjectAsync(request.ProjectId, currentUserId, cancellationToken))
        {
            return null;
        }

        var task = new TaskItem
        {
            ProjectId = request.ProjectId,
            CreatedById = currentUserId,
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

    public async Task<IReadOnlyList<TaskResponse>> GetAllAsync(int currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var query = ProjectTask();
        if (!isAdmin)
        {
            query = query.Where(t => dbContext.Projects.Any(p => p.Id == t.ProjectId && p.OwnerId == currentUserId)
                || dbContext.TaskAssignments.Any(a => a.TaskId == t.Id && a.UserId == currentUserId));
        }

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<TaskResponse?> GetByIdAsync(int id, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        return ProjectTask()
            .Where(t => t.Id == id && (isAdmin
                || dbContext.Projects.Any(p => p.Id == t.ProjectId && p.OwnerId == currentUserId)
                || dbContext.TaskAssignments.Any(a => a.TaskId == t.Id && a.UserId == currentUserId)))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<TaskResponse?> UpdateAsync(int id, UpdateTaskRequest request, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var task = await dbContext.Tasks
            .SingleOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);

        if (task is null
            || (!isAdmin && !await CanManageTaskAsync(task, currentUserId, cancellationToken))
            || (!isAdmin && request.ProjectId != task.ProjectId)
            || (isAdmin && !await ProjectExistsAsync(request.ProjectId, cancellationToken)))
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

    public async Task<bool> DeleteAsync(int id, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var task = await dbContext.Tasks
            .SingleOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);

        if (task is null || (!isAdmin && !await CanManageTaskAsync(task, currentUserId, cancellationToken)))
        {
            return false;
        }

        task.IsDeleted = true;
        task.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<bool> CanManageTaskAsync(TaskItem task, int currentUserId, CancellationToken cancellationToken)
    {
        return await dbContext.Projects.AnyAsync(p => p.Id == task.ProjectId && p.OwnerId == currentUserId, cancellationToken)
            || await dbContext.TaskAssignments.AnyAsync(a => a.TaskId == task.Id && a.UserId == currentUserId, cancellationToken);
    }

    private Task<bool> CanManageProjectAsync(int projectId, int currentUserId, CancellationToken cancellationToken)
    {
        return dbContext.Projects.AnyAsync(p => p.Id == projectId && !p.IsArchived && p.OwnerId == currentUserId, cancellationToken);
    }

    private Task<bool> ProjectExistsAsync(int projectId, CancellationToken cancellationToken)
    {
        return dbContext.Projects.AnyAsync(p => p.Id == projectId && !p.IsArchived, cancellationToken);
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
