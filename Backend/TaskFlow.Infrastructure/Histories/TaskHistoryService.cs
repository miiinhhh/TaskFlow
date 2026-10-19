using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Histories;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Histories;

public sealed class TaskHistoryService(TaskFlowDbContext dbContext) : ITaskHistoryService
{
    public async Task<TaskHistoryResponse?> CreateAsync(int taskId, int currentUserId, bool isAdmin, CreateTaskHistoryRequest request, CancellationToken cancellationToken = default)
    {
        if (!await CanManageTaskAsync(taskId, currentUserId, isAdmin, cancellationToken))
        {
            return null;
        }

        var history = new TaskHistory
        {
            TaskId = taskId,
            UserId = currentUserId,
            Action = request.Action.Trim(),
            OldValue = request.OldValue,
            NewValue = request.NewValue,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.TaskHistories.Add(history);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await ProjectHistory()
            .SingleAsync(h => h.Id == history.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<TaskHistoryResponse>?> GetByTaskIdAsync(int taskId, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        if (!await CanViewTaskAsync(taskId, currentUserId, isAdmin, cancellationToken))
        {
            return null;
        }

        return await ProjectHistory()
            .Where(h => h.TaskId == taskId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<TaskHistoryResponse?> GetByIdAsync(int id, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var taskId = await dbContext.TaskHistories
            .Where(h => h.Id == id)
            .Select(h => (int?)h.TaskId)
            .SingleOrDefaultAsync(cancellationToken);

        if (!taskId.HasValue || !await CanViewTaskAsync(taskId.Value, currentUserId, isAdmin, cancellationToken))
        {
            return null;
        }

        return await ProjectHistory().SingleAsync(h => h.Id == id, cancellationToken);
    }

    private IQueryable<TaskHistoryResponse> ProjectHistory()
    {
        return dbContext.TaskHistories
            .AsNoTracking()
            .Select(h => new TaskHistoryResponse(
                h.Id,
                h.TaskId,
                h.Task == null ? null : h.Task.Title,
                h.UserId,
                h.User == null ? null : h.User.FullName,
                h.Action,
                h.OldValue,
                h.NewValue,
                h.CreatedAt));
    }

    private Task<bool> CanViewTaskAsync(int taskId, int currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        return dbContext.Tasks.AnyAsync(t =>
            t.Id == taskId
            && !t.IsDeleted
            && dbContext.Projects.Any(p => p.Id == t.ProjectId && !p.IsArchived)
            && (isAdmin
                || dbContext.Projects.Any(p => p.Id == t.ProjectId && p.OwnerId == currentUserId)
                || (dbContext.ProjectMembers.Any(m => m.ProjectId == t.ProjectId && m.UserId == currentUserId)
                    && dbContext.TaskAssignments.Any(a => a.TaskId == t.Id && a.UserId == currentUserId))),
            cancellationToken);
    }

    private Task<bool> CanManageTaskAsync(int taskId, int currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        return dbContext.Tasks.AnyAsync(t =>
            t.Id == taskId
            && !t.IsDeleted
            && dbContext.Projects.Any(p => p.Id == t.ProjectId && !p.IsArchived && (isAdmin || p.OwnerId == currentUserId)),
            cancellationToken);
    }
}
