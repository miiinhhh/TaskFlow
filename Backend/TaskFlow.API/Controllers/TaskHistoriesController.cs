using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskFlow.Application.Histories;

namespace TaskFlow.API.Controllers;

[ApiController]
[Authorize]
[Produces("application/json")]
public sealed class TaskHistoriesController(ITaskHistoryService historyService) : ControllerBase
{
    [HttpPost("api/tasks/{taskId:int}/histories")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<TaskHistoryResponse>> Create(
        int taskId,
        CreateTaskHistoryRequest request,
        CancellationToken cancellationToken)
    {
        if (CurrentUserId <= 0)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.Action))
        {
            return BadRequest("Action is required.");
        }

        try
        {
            var history = await historyService.CreateAsync(taskId, CurrentUserId, IsAdmin, request, cancellationToken);
            if (history is null)
            {
                return Forbid();
            }

            return CreatedAtAction(nameof(GetById), new { id = history.Id }, history);
        }
        catch (DbUpdateException)
        {
            return BadRequest("Task or user does not exist.");
        }
    }

    [HttpGet("api/tasks/{taskId:int}/histories")]
    public async Task<ActionResult<IReadOnlyList<TaskHistoryResponse>>> GetByTaskId(int taskId, CancellationToken cancellationToken)
    {
        var histories = await historyService.GetByTaskIdAsync(taskId, CurrentUserId, IsAdmin, cancellationToken);
        return histories is null ? NotFound() : Ok(histories);
    }

    [HttpGet("api/task-histories/{id:int}")]
    public async Task<ActionResult<TaskHistoryResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var history = await historyService.GetByIdAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return history is null ? NotFound() : Ok(history);
    }

    private int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    private bool IsAdmin => User.IsInRole("Admin");
}
