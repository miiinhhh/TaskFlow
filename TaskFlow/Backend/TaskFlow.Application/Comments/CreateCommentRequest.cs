namespace TaskFlow.Application.Comments;

public sealed record CreateCommentRequest(
    int TaskId,
    int UserId,
    string Content);
