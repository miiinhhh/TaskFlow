namespace TaskFlow.Application.Comments;

public interface ICommentService
{
    Task<CommentResponse?> CreateAsync(int taskId, int currentUserId, bool isAdmin, CreateCommentRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CommentResponse>?> GetByTaskIdAsync(int taskId, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<CommentResponse?> GetByIdAsync(int id, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<CommentResponse?> UpdateAsync(int id, int currentUserId, bool isAdmin, UpdateCommentRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
}
