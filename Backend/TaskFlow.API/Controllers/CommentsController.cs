using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Comments;

namespace TaskFlow.API.Controllers;

[ApiController]
[Produces("application/json")]
public sealed class CommentsController(ICommentService commentService) : ControllerBase
{
    [HttpPost("api/tasks/{taskId:int}/comments")]
    public async Task<ActionResult<CommentResponse>> Create(
        int taskId,
        CreateCommentRequest request,
        CancellationToken cancellationToken)
    {
        if (taskId != request.TaskId)
        {
            return BadRequest("Route task id must match request task id.");
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest("Content is required.");
        }

        try
        {
            var comment = await commentService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = comment.Id }, comment);
        }
        catch (DbUpdateException)
        {
            return BadRequest("Task or user does not exist.");
        }
    }

    [HttpGet("api/tasks/{taskId:int}/comments")]
    public async Task<ActionResult<IReadOnlyList<CommentResponse>>> GetByTaskId(int taskId, CancellationToken cancellationToken)
    {
        return Ok(await commentService.GetByTaskIdAsync(taskId, cancellationToken));
    }

    [HttpGet("api/comments/{id:int}")]
    public async Task<ActionResult<CommentResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var comment = await commentService.GetByIdAsync(id, cancellationToken);
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

        var comment = await commentService.UpdateAsync(id, request, cancellationToken);
        return comment is null ? NotFound() : Ok(comment);
    }

    [HttpDelete("api/comments/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await commentService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
