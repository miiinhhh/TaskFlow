using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Comments;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Comments;

public sealed class CommentService(TaskFlowDbContext dbContext) : ICommentService
{
    public async Task<CommentResponse> CreateAsync(CreateCommentRequest request, CancellationToken cancellationToken = default)
    {
        var comment = new TaskComment
        {
            TaskId = request.TaskId,
            UserId = request.UserId,
            Content = request.Content.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        dbContext.TaskComments.Add(comment);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await ProjectComment()
            .SingleAsync(c => c.Id == comment.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<CommentResponse>> GetByTaskIdAsync(int taskId, CancellationToken cancellationToken = default)
    {
        return await ProjectComment()
            .Where(c => c.TaskId == taskId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<CommentResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return ProjectComment()
            .SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<CommentResponse?> UpdateAsync(int id, UpdateCommentRequest request, CancellationToken cancellationToken = default)
    {
        var comment = await dbContext.TaskComments
            .SingleOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

        if (comment is null)
        {
            return null;
        }

        comment.Content = request.Content.Trim();
        comment.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return await ProjectComment()
            .SingleAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var comment = await dbContext.TaskComments
            .SingleOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

        if (comment is null)
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
}
