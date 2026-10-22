using System.ComponentModel.DataAnnotations;
using TaskFlow.Application.Validation;

namespace TaskFlow.Application.Comments;

public sealed record UpdateCommentRequest(
    [param: Required, NotWhiteSpace, StringLength(4000)] string Content);
