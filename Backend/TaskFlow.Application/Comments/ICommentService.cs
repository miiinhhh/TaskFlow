namespace TaskFlow.Application.Comments;

public interface ICommentService
{
    Task<CommentResponse> CreateAsync(CreateCommentRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CommentResponse>> GetByTaskIdAsync(int taskId, CancellationToken cancellationToken = default);
    Task<CommentResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CommentResponse?> UpdateAsync(int id, UpdateCommentRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
