using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Tasks;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Tasks;

public sealed class TaskService(TaskFlowDbContext dbContext) : ITaskService
{
    public async Task<TaskResponse?> CreateAsync(CreateTaskRequest request, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var projectExists = await ProjectExistsAsync(request.ProjectId, cancellationToken);
        if (!projectExists)
        {
            throw new TaskValidationException($"ProjectId {request.ProjectId} does not exist or is archived.");
        }

        if (!isAdmin && !await CanManageProjectAsync(request.ProjectId, currentUserId, cancellationToken))
        {
            return null;
        }

        await ValidateReferencesAsync(request.StatusId, request.PriorityId, cancellationToken);

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

        return await ProjectTask(task.Id)
            .SingleAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TaskResponse>> GetAllAsync(int currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Tasks
            .AsNoTracking()
            .Where(t => !t.IsDeleted);

        if (!isAdmin)
        {
            query = query.Where(t =>
                dbContext.Projects.Any(p => p.Id == t.ProjectId && p.OwnerId == currentUserId)
                || dbContext.TaskAssignments.Any(a => a.TaskId == t.Id && a.UserId == currentUserId));
        }

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TaskResponse(
                t.Id,
                t.ProjectId,
                t.Project!.Name,
                t.CreatedById,
                t.CreatedBy!.FullName,
                t.StatusId,
                t.Status!.Name,
                t.PriorityId,
                t.Priority!.Name,
                t.Title,
                t.Description,
                t.StartDate,
                t.DueDate,
                t.EstimatedHours,
                t.ActualHours,
                t.CreatedAt,
                t.UpdatedAt,
                t.CompletedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<TaskResponse?> GetByIdAsync(int id, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Tasks
            .AsNoTracking()
            .Where(t => t.Id == id && !t.IsDeleted);

        if (!isAdmin)
        {
            query = query.Where(t =>
                dbContext.Projects.Any(p => p.Id == t.ProjectId && p.OwnerId == currentUserId)
                || dbContext.TaskAssignments.Any(a => a.TaskId == t.Id && a.UserId == currentUserId));
        }

        return await query
            .Select(t => new TaskResponse(
                t.Id,
                t.ProjectId,
                t.Project!.Name,
                t.CreatedById,
                t.CreatedBy!.FullName,
                t.StatusId,
                t.Status!.Name,
                t.PriorityId,
                t.Priority!.Name,
                t.Title,
                t.Description,
                t.StartDate,
                t.DueDate,
                t.EstimatedHours,
                t.ActualHours,
                t.CreatedAt,
                t.UpdatedAt,
                t.CompletedAt))
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

        await ValidateReferencesAsync(request.StatusId, request.PriorityId, cancellationToken);

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

        return await ProjectTask(id)
            .SingleAsync(cancellationToken);
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

    private async Task ValidateReferencesAsync(int statusId, int priorityId, CancellationToken cancellationToken)
    {
        if (!await dbContext.TaskStatuses.AnyAsync(s => s.Id == statusId, cancellationToken))
        {
            throw new TaskValidationException($"StatusId {statusId} does not exist.");
        }

        if (!await dbContext.TaskPriorities.AnyAsync(p => p.Id == priorityId, cancellationToken))
        {
            throw new TaskValidationException($"PriorityId {priorityId} does not exist.");
        }
    }

    private IQueryable<TaskResponse> ProjectTask(int? id = null)
    {
        var query = dbContext.Tasks
            .AsNoTracking()
            .Where(t => !t.IsDeleted);

        if (id.HasValue)
        {
            query = query.Where(t => t.Id == id.Value);
        }

        return query
            .Select(t => new TaskResponse(
                t.Id,
                t.ProjectId,
                t.Project!.Name,
                t.CreatedById,
                t.CreatedBy!.FullName,
                t.StatusId,
                t.Status!.Name,
                t.PriorityId,
                t.Priority!.Name,
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
