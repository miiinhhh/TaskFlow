namespace TaskFlow.Application.Comments;

public sealed record CommentResponse(
    int Id,
    int TaskId,
    string? TaskTitle,
    int UserId,
    string? UserFullName,
    string Content,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
