using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Application.Comments;

public sealed record UpdateCommentRequest(
    [property: Required, StringLength(4000)] string Content);
