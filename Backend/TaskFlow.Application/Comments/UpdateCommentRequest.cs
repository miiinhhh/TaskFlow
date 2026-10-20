using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Application.Comments;

public sealed record UpdateCommentRequest(
    [param: Required, StringLength(4000)] string Content);
