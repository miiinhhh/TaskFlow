using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskFlow.Application.Comments;

namespace TaskFlow.API.Controllers;

[ApiController]
[Authorize]
[Produces("application/json")]
public sealed class CommentsController(ICommentService commentService) : ControllerBase
{
    [HttpPost("api/tasks/{taskId:int}/comments")]
    public async Task<ActionResult<CommentResponse>> Create(
        int taskId,
        CreateCommentRequest request,
        CancellationToken cancellationToken)
    {
        if (CurrentUserId <= 0)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest("Content is required.");
        }

        var comment = await commentService.CreateAsync(
            taskId,
            CurrentUserId,
            IsAdmin,
            request,
            cancellationToken);
        if (comment is null)
        {
            return Forbid();
        }

        return CreatedAtAction(nameof(GetById), new { id = comment.Id }, comment);
    }

    [HttpGet("api/tasks/{taskId:int}/comments")]
    public async Task<ActionResult<IReadOnlyList<CommentResponse>>> GetByTaskId(int taskId, CancellationToken cancellationToken)
    {
        var comments = await commentService.GetByTaskIdAsync(taskId, CurrentUserId, IsAdmin, cancellationToken);
        return comments is null ? NotFound() : Ok(comments);
    }

    [HttpGet("api/comments/{id:int}")]
    public async Task<ActionResult<CommentResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var comment = await commentService.GetByIdAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return comment is null ? NotFound() : Ok(comment);
    }

    [HttpPut("api/comments/{id:int}")]
    public async Task<ActionResult<CommentResponse>> Update(
        int id,
        UpdateCommentRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest("Content is required.");
        }

        var comment = await commentService.UpdateAsync(id, CurrentUserId, IsAdmin, request, cancellationToken);
        return comment is null ? NotFound() : Ok(comment);
    }

    [HttpDelete("api/comments/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await commentService.DeleteAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    private int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    private bool IsAdmin => User.IsInRole("Admin");
}
