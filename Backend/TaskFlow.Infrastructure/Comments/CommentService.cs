using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Comments;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Comments;

public sealed class CommentService(TaskFlowDbContext dbContext) : ICommentService
{
    public async Task<CommentResponse?> CreateAsync(int taskId, int currentUserId, bool isAdmin, CreateCommentRequest request, CancellationToken cancellationToken = default)
    {
        if (!await CanViewTaskAsync(taskId, currentUserId, isAdmin, cancellationToken))
        {
            return null;
        }

        var comment = new TaskComment
        {
            TaskId = taskId,
            UserId = currentUserId,
            Content = request.Content.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        dbContext.TaskComments.Add(comment);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await ProjectComment()
            .SingleAsync(c => c.Id == comment.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<CommentResponse>?> GetByTaskIdAsync(int taskId, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        if (!await CanViewTaskAsync(taskId, currentUserId, isAdmin, cancellationToken))
        {
            return null;
        }

        return await ProjectComment()
            .Where(c => c.TaskId == taskId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<CommentResponse?> GetByIdAsync(int id, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var taskId = await dbContext.TaskComments
            .Where(c => c.Id == id && !c.IsDeleted)
            .Select(c => (int?)c.TaskId)
            .SingleOrDefaultAsync(cancellationToken);

        if (!taskId.HasValue || !await CanViewTaskAsync(taskId.Value, currentUserId, isAdmin, cancellationToken))
        {
            return null;
        }

        return await ProjectComment().SingleAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<CommentResponse?> UpdateAsync(int id, int currentUserId, bool isAdmin, UpdateCommentRequest request, CancellationToken cancellationToken = default)
    {
        var comment = await dbContext.TaskComments
            .SingleOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

        if (comment is null || !await CanManageCommentAsync(comment, currentUserId, isAdmin, cancellationToken))
        {
            return null;
        }

        comment.Content = request.Content.Trim();
        comment.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return await ProjectComment()
            .SingleAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(int id, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var comment = await dbContext.TaskComments
            .SingleOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

        if (comment is null || !await CanManageCommentAsync(comment, currentUserId, isAdmin, cancellationToken))
        {
            return false;
        }

        comment.IsDeleted = true;
        comment.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private IQueryable<CommentResponse> ProjectComment()
    {
        return dbContext.TaskComments
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .Select(c => new CommentResponse(
                c.Id,
                c.TaskId,
                c.Task == null ? null : c.Task.Title,
                c.UserId,
                c.User == null ? null : c.User.FullName,
                c.Content,
                c.CreatedAt,
                c.UpdatedAt));
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

    private Task<bool> CanManageCommentAsync(TaskComment comment, int currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        if (isAdmin || comment.UserId == currentUserId)
        {
            return Task.FromResult(true);
        }

        return dbContext.Tasks.AnyAsync(t =>
            t.Id == comment.TaskId
            && !t.IsDeleted
            && dbContext.Projects.Any(p => p.Id == t.ProjectId && !p.IsArchived && p.OwnerId == currentUserId),
            cancellationToken);
    }
}
