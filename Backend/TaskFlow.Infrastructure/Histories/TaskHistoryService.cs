using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Histories;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Histories;

public sealed class TaskHistoryService(TaskFlowDbContext dbContext) : ITaskHistoryService
{
    public async Task<TaskHistoryResponse> CreateAsync(CreateTaskHistoryRequest request, CancellationToken cancellationToken = default)
    {
        var history = new TaskHistory
        {
            TaskId = request.TaskId,
            UserId = request.UserId,
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

    public async Task<IReadOnlyList<TaskHistoryResponse>> GetByTaskIdAsync(int taskId, CancellationToken cancellationToken = default)
    {
        return await ProjectHistory()
            .Where(h => h.TaskId == taskId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<TaskHistoryResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return ProjectHistory()
            .SingleOrDefaultAsync(h => h.Id == id, cancellationToken);
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
}
