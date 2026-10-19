using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Application.Comments;

public sealed record CreateCommentRequest(
    [property: Required, StringLength(4000)] string Content);
