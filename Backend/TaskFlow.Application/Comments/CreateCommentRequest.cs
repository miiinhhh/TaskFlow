using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Application.Comments;

public sealed record CreateCommentRequest(
    [param: Required, StringLength(4000)] string Content);
